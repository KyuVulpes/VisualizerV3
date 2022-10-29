using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VisualizerV3.Shared;
using CompressionLevel = System.IO.Compression.CompressionLevel;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace VisualizerV3.Editor {
	public static class Build {
		[Flags]
		private enum Includes : byte {
			Nothing     = 0,
			Json        = 0b0000_0001,
			Image       = 0b0000_0010,
			Manifest    = 0b0000_0100,
			AssetBundle = 0b0000_1000,
		}

		private struct ProjectDetails {
			public string projectName;
			public string version;
			public string author;
			public string copyright;
			public string icon;
		}

		private static readonly string BUILD_LOCATION = Path.Combine( Application.dataPath, "..", "Build" );

		[MenuItem( "Build/Build Everything" )]
		public static void BuildEverything() {
			var levels = new[] {
				"Assets/Scenes/Startup.unity", "Assets/Scenes/Core.unity", "Assets/Scenes/Audio.unity",
			};
			var report = BuildPipeline.BuildPlayer( levels, Path.Combine( BUILD_LOCATION, "Wallpaper Visualizer.exe" ), BuildTarget.StandaloneWindows64, BuildOptions.None );

			// Checks to see if the build process should continue to also include the .NET project.
			if ( report.summary.result != BuildResult.Succeeded ) {
				return;
			}
		}

		[MenuItem( "Build/Export To AssetBundle" )]
		public static void BuildAssetBundle() {
			/* First sees if the user has selected the GameObject that is meant to be put into the AssetBundle.
			 * If the user hasn't, it then checks the scene to see if there is a game object with ObjectDetails.
			 * Finally, if both don't return anything, it will output the message to the user and log the error.
			 *
			 * If it does find one, it then asks the user where to store the file and passes the GameObject to
			 * the method that builds the AssetBundles. */
			var asset = Selection.objects.FirstOrDefault( o => o is GameObject go && go.GetComponent<ObjectData>() != null ) as GameObject;

			if ( asset == null ) {
				asset = Object.FindObjectOfType<ObjectData>()?.gameObject;

				if ( asset == null ) {
					Debug.LogError( "No GameObject found with the ObjectDescription enabled." );
					EditorUtility.DisplayDialog( "No Asset found!", "No GameObject found with the Object Description component on it.", "Ok" );

					return;
				}
			}

			var saveFolder = EditorUtility.SaveFolderPanel( "Save AssetBundle...", string.Empty, string.Empty );

			// Transfers all of the information from the ObjectData to a struct that represents the data.
			var componentDetails = asset.GetComponent<ObjectData>();
			var details = new ProjectDetails {
				author      = componentDetails.Author,
				projectName = componentDetails.ProjectName,
				version     = componentDetails.Version,
				copyright   = componentDetails.Copyright,
				icon        = AssetDatabase.GetAssetPath( componentDetails.Icon ),
			};

			// Creates a new object to modify.
			var toSave = Object.Instantiate( asset );

			// Removes the Object Data since it is no longer usefull.
			componentDetails = toSave.GetComponent<ObjectData>();
			Object.DestroyImmediate( componentDetails );

			// Now builds the object into an AssetBundle.
			BuildToAssetBundle( toSave, details, saveFolder, out var abPath, out var manifestPath );

			Object.DestroyImmediate( toSave );

			var json = JsonConvert.SerializeObject(
				details,
				Formatting.None,
				new JsonSerializerSettings {
					Formatting            = Formatting.None,
					MaxDepth              = 1,
					NullValueHandling     = NullValueHandling.Ignore,
					ReferenceLoopHandling = ReferenceLoopHandling.Error,
				}
			);

			ConvertToStoreFile( saveFolder, abPath, manifestPath, json, details );
			
			File.Delete( abPath );
			File.Delete( manifestPath );
		}

		private static void ConvertToStoreFile( string saveFolder, string assetBundlePath, string manifestPath, string json, ProjectDetails details ) {
			using var fStream        = new FileStream( Path.Combine( saveFolder, $"{details.projectName}.pak" ), FileMode.Create, FileAccess.Write, FileShare.Read );
			using var compressStream = new GZipStream( fStream, CompressionLevel.Optimal );
			using var binWriter      = new BinaryWriter( compressStream, Encoding.Unicode );
			var       includes       = Includes.Json | Includes.AssetBundle | Includes.Manifest;

			fStream.Write(
				new byte[] {
					0xe6, 0x21, // Magic Number
				}
			);

			if ( !string.IsNullOrWhiteSpace( details.icon ) ) {
				includes |= Includes.Image;
			}

			binWriter.Write( ( byte )includes );
			binWriter.Write( json );

			if ( !string.IsNullOrWhiteSpace( details.icon ) ) {
				using var fStream2 = new FileStream( details.icon, FileMode.Open, FileAccess.Read );

				binWriter.Write( fStream2.Length );

				fStream2.CopyTo( compressStream );

			}
			
			using ( var fStream2 = new FileStream( manifestPath, FileMode.Open, FileAccess.Read ) ) {
				binWriter.Write( fStream2.Length );

				fStream2.CopyTo( compressStream );
			}
			
			using ( var fStream2 = new FileStream( assetBundlePath, FileMode.Open, FileAccess.Read ) ) {
				binWriter.Write( fStream2.Length );

				fStream2.CopyTo( compressStream );
			}
		}

		private static void BuildToAssetBundle( GameObject asset, ProjectDetails details, string buildTo, out string assetBundlePath, out string manifestPath ) {
			// This prepares everything for building the AssetBundle.
			// The reason the prefab is called `Assets/main.prefab` is for the program to easily find and load the prefab without extra steps.
			var prefabFile      = $"Assets/main.prefab";
			var assetBundleName = $"{details.projectName}.assetbundle";
			var buildMap = new AssetBundleBuild {
				assetNames = new[] {
					prefabFile,
				},
				assetBundleName = assetBundleName,
			};

			// This stores the position of the asset before it gets reset to 0 on both the X and Z axis.
			var oldPos = asset.transform.position;

			asset.transform.position = new Vector3( 0f, oldPos.y, 0f );

			// Saves the asset as a prefab temporarily into the Assets folder so the bundle can be built.
			_ = PrefabUtility.SaveAsPrefabAsset( asset, prefabFile );

			_ = BuildPipeline.BuildAssetBundles(
				buildTo,
				new[] {
					buildMap,
				},
				BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
				BuildTarget.StandaloneWindows64
			);

			assetBundlePath = Path.Combine( buildTo, assetBundleName );
			manifestPath    = assetBundlePath + ".manifest";

			// Reverts the asset back to the old position.
			// ReSharper disable once Unity.InefficientPropertyAccess
			asset.transform.position = oldPos;

			// This is meant to clear out the prefab from the project, as it is no longer needed.
			File.Delete( Path.Combine( Application.dataPath, $"main.prefab" ) );
			File.Delete( Path.Combine( Application.dataPath, $"main.prefab.meta" ) );
		}
	}
}

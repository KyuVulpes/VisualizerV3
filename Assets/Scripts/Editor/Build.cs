using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
// using UnityWallpaper.Stylizers;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace VisualizerV3.Editor {
	public static class Build {
		private const string DOTNET_ARGS = "build" +
		                                   " /property:TargetFramework=net5.0-windows" +
		                                   " /property:RuntimeIdentifier=win-x64" +
										   " -c Release"+
		                                   " \"/property:PublicDir={0}\"" +
		                                   " /t:Publish" +
		                                   " -v diag";

		private static readonly string BUILD_LOCATION = Path.Combine( Application.dataPath, "..", "Build" );

		[MenuItem( "Build/Build Everything" )]
		public static void BuildEverything() {
			var levels = new[] {
				"Assets/Scenes/Startup.unity",
				"Assets/Scenes/Core.unity",
				"Assets/Scenes/Audio.unity",
			};
			var report = BuildPipeline.BuildPlayer(
				levels,
				Path.Combine( BUILD_LOCATION, "Wallpaper Visualizer.exe" ),
				BuildTarget.StandaloneWindows64,
				BuildOptions.None
			);
			var dotnetBuildLocation = Path.Combine(
				Application.dataPath,
				"..",
				"AudioHandler",
				"bin",
				"Release",
				"net5.0-windows",
				"win-x64",
				"publish"
			);

			// Checks to see if the build process should continue to also include the .NET project.
			if ( report.summary.result != BuildResult.Succeeded ) {
				return;
			}
		}

		// [MenuItem( "Build/Export To AssetBundle" )]
		// public static void BuildAssetBundle() {
		// 	/* First sees if the user has selected the GameObject that is meant to be put into the AssetBundle.
		// 	 * If the user hasn't, it then checks to scene to see if there is a game object with ObjectDetails.
		// 	 * Finally, if both don't return anything, it will output the message to the user and log the error.
		// 	 *
		// 	 * If it does find one, it then asks the user where to store the file and passes the GameObject to
		// 	 * the method that builds the AssetBundles. */
		// 	var asset = Selection.objects.FirstOrDefault( o => o is GameObject go && go.GetComponent<ObjectDetails>() != null ) as GameObject;
		//
		// 	if ( asset == null ) {
		// 		asset = Object.FindObjectOfType<ObjectDetails>()?.gameObject;
		//
		// 		if ( asset == null ) {
		// 			Debug.LogError( "No GameObject found with the ObjectDescription enabled." );
		// 			EditorUtility.DisplayDialog( "No Asset found!", "No GameObject found with the Object Description component on it.", "Ok" );
		//
		// 			return;
		// 		}
		// 	}
		//
		// 	var saveFolder = EditorUtility.SaveFolderPanel( "Save AssetBundle...", string.Empty, string.Empty );
		//
		// 	BuildToAssetBundle( asset.gameObject, saveFolder );
		// }
		//
		// // This method is meant to find all objects that are meant to go into an asset bundle and build them into there.
		// [MenuItem( "Build/All Details To Bundle" )]
		// public static void BuildAllToAssetBundle() {
		// 	// Looks for all objects that have ObjectDetails on them.
		// 	var assets = Object.FindObjectsOfType<ObjectDetails>();
		//
		// 	// Checks to make sure that it found any. And if it didn't to report that it didn't as an error and to the user.
		// 	if ( assets == null || assets.Length == 0 ) {
		// 		Debug.LogError( "No GameObject found with the ObjectDescription enabled." );
		// 		EditorUtility.DisplayDialog( "No Asset found!", "No GameObject found with the Object Description component on it.", "Ok" );
		//
		// 		return;
		// 	}
		//
		// 	// Asks the user where to save it, the program will auto-generate a name for the bundle.
		// 	var saveFolder = EditorUtility.SaveFolderPanel( "Save AssetBundles...", string.Empty, string.Empty );
		//
		// 	// Looks through each asset to build into an asset bundle and builds them.
		// 	foreach ( var asset in assets ) {
		// 		BuildToAssetBundle( asset.gameObject, saveFolder );
		// 	}
		// }

		private static void BuildToAssetBundle( GameObject asset, string buildTo ) {
			// This prepares everything for building the AssetBundle.
			// The reason the prefab is called `Assets/main.prefab` is for the program to easily find and load the prefab without extra steps.
			var prefabFile      = $"Assets/main.prefab";
			var assetBundleName = $"{asset.name}.assetbundle";
			var buildMap = new AssetBundleBuild {
				assetNames      = new[] { prefabFile },
				assetBundleName = assetBundleName,
			};

			// This stores the position of the asset before it gets reset to 0 on both the X and Z axis.
			var oldPos = asset.transform.position;

			asset.transform.position = new Vector3( 0f, oldPos.y, 0f );

			// Saves the asset as a prefab temporarily into the Assets folder so the bundle can be built.
			_ = PrefabUtility.SaveAsPrefabAsset( asset.gameObject, prefabFile );

			_ = BuildPipeline.BuildAssetBundles( buildTo, new[] { buildMap }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64 );

			// Reverts the asset back to the old position.
			// ReSharper disable once Unity.InefficientPropertyAccess
			asset.transform.position = oldPos;

			// This is meant to clear out the prefab from the project, as it is no longer needed.
			File.Delete( Path.Combine( Application.dataPath, $"main.prefab" ) );
			File.Delete( Path.Combine( Application.dataPath, $"main.prefab.meta" ) );

			// This cleans up the output location to help ensure that it is clean.
			foreach ( var file in Directory.EnumerateFiles( buildTo, "*" ) ) {
				if ( file.EndsWith( ".assetbundle", StringComparison.OrdinalIgnoreCase ) ) {
					continue;
				}

				File.Delete( file );
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace VisualizerV3.Visual {
	public class Spawner : MonoBehaviour {
		[Flags]
		private enum Includes : byte {
			Nothing     = 0,
			Json        = 0b0000_0001,
			Image       = 0b0000_0010,
			Manifest    = 0b0000_0100,
			AssetBundle = 0b0000_1000,
		}

		private GameObject spawned;

		// Start is called before the first frame update
		private void Start() {
			LoadPackageFile( Path.Combine( Application.dataPath, "..", "Rex Kyuubi.pak" ) );
		}

		// Update is called once per frame
		private void Update() {

		}

		private void LoadPackageFile( string file ) {
			using var inMemFile = LoadAndDecompress( file );
			using var reader    = new BinaryReader( inMemFile, Encoding.Unicode );
			var       includes  = ( Includes )reader.ReadByte();

			if ( !includes.HasFlag( Includes.Json ) || !includes.HasFlag( Includes.AssetBundle ) || !includes.HasFlag( Includes.Manifest ) ) {
				throw new Exception( "Package does not have the required components." );
			}

			_ = reader.ReadString();

			if ( includes.HasFlag( Includes.Image ) ) {
				var skipLength = reader.ReadInt64();

				inMemFile.Position += skipLength;
			}

			var fSize     = reader.ReadInt64();
			var tmpBuffer = new byte[fSize];

			inMemFile.Read( tmpBuffer );

			using var manifestStream = new MemoryStream( tmpBuffer );

			fSize     = reader.ReadInt64();
			tmpBuffer = new byte[fSize];

			inMemFile.Read( tmpBuffer );

			using var abStream = new MemoryStream( tmpBuffer );
			var       ab       = AssetBundle.LoadFromStream( abStream );
			var       toSpawn  = ab.LoadAsset<GameObject>( "Assets/main.prefab" ); 
			
			spawned = Instantiate( toSpawn, transform );
		}

		private Stream LoadAndDecompress( string file ) {
			using var fStream = new FileStream( file, FileMode.Open, FileAccess.Read, FileShare.Read );

			if ( fStream.ReadByte() != 0xe6 || fStream.ReadByte() != 0x21 ) {
				throw new IOException( "File is not a package file." );
			}

			var       memStream  = new MemoryStream();
			using var decompress = new GZipStream( fStream, CompressionMode.Decompress );

			decompress.CopyTo( memStream );

			memStream.Position = 0;

			return memStream;
		}
	}
}

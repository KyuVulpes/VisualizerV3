using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VisualizerV3.Visual {
	public class Spawner : MonoBehaviour {
		/* This class load the Package (.pak) file. The Package File Type contains the following data:
		 * JSON        - The information about who made it, the copyright, version, and license.
		 * Image       - The Thumbnail for the package.
		 * Manifest    - Information about the AssetBundle. Is currently not used.
		 * AssetBundle - The embedded file that contains the actual data.
		 *
		 * The Package file is laid out as following:
		 *
		 * [Magic Number][Compressed Data]
		 *
		 * The compression used is GZip, and inside the compressed section is as follows:
		 * [.NET Binary Writer String|JSON][[Int64][Image](optional)][[Int64][Manifest]][[Int64][AssetBundle]]
		 *
		 * The image is optional, however the Manifest and AssetBundle (along with the JSON) are all required.
		 * If one of those is missing than it is considered an incomplete Package file and is meant to not continue loading.
		 */

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
			
		}

		// Update is called once per frame
		private void Update() {

		}

		private async Task LoadPackageFile( string file ) {
			await using var inMemFile = await LoadAndDecompress( file );
			using var       reader    = new BinaryReader( inMemFile, Encoding.Unicode );
			var             includes  = ( Includes )reader.ReadByte();

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

			_ = await inMemFile.ReadAsync( tmpBuffer );

			using var manifestStream = new MemoryStream( tmpBuffer );

			fSize     = reader.ReadInt64();
			tmpBuffer = new byte[fSize];

			_ = await inMemFile.ReadAsync( tmpBuffer );

			using var abStream = new MemoryStream( tmpBuffer );
			var       ab       = AssetBundle.LoadFromStream( abStream );
			var       toSpawn  = ab.LoadAsset<GameObject>( "Assets/main.prefab" );

			spawned = Instantiate( toSpawn, transform );
		}

		private static async Task<Stream> LoadAndDecompress( string file ) {
			await using var fStream = new FileStream( file, FileMode.Open, FileAccess.Read, FileShare.Read );

			if ( fStream.ReadByte() != 0xe6 || fStream.ReadByte() != 0x21 ) {
				throw new IOException( "File is not a package file." );
			}

			var             memStream  = new MemoryStream();
			await using var decompress = new GZipStream( fStream, CompressionMode.Decompress );

			await decompress.CopyToAsync( memStream );

			memStream.Position = 0;

			return memStream;
		}
	}
}

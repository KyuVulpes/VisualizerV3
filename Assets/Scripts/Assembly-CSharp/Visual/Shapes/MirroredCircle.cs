using System.Text;
using UnityEngine;

namespace VisualizerV3.Audio.Shapes {
	public class MirroredCircle : IVisualizeShape {

		public void GenerateShape( in int count, in float radius, in Vector3 parentPos, out Vector3[] posArray, out Quaternion[] rotationArray, out int[] barNums ) {
			var amountToSpawn = count * 2;
			var offset        = 1;

			posArray      = new Vector3[amountToSpawn];
			rotationArray = new Quaternion[amountToSpawn];
			barNums       = new int[amountToSpawn];

			for ( var i = 0; i < amountToSpawn; ++i ) {
				var theta = ( ( 2 * Mathf.PI ) / amountToSpawn ) * i;
				var x     = radius * Mathf.Sin( theta );
				var y     = radius * Mathf.Cos( theta );
				var pos   = new Vector3( x, y, 0 );
				var angle = Quaternion.LookRotation( parentPos - pos, Vector3.forward );

				angle.eulerAngles = new Vector3() {
					x = angle.eulerAngles.x - 90f,
					y = angle.eulerAngles.y,
					z = 0f,
				};

				posArray[i]      = pos;
				rotationArray[i] = angle;
				barNums[i]       = i < count ? i : i - ( ( i % count ) + offset++ );
			}

			var builder = new StringBuilder().AppendLine( $"barNums Length:\t{barNums.Length}" ).Append( "{ " );

			for ( var i = 0; i < barNums.Length; ++i ) {
				builder.Append( barNums[i] );

				if ( i < barNums.Length - 1 ) {
					builder.Append( ", " );
				}
			}

			Debug.Log( builder.Append( " }" ).ToString() );
		}
	}
}

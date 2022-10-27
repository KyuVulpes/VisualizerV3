using System.Text;
using UnityEngine;

namespace VisualizerV3.Visual.Shapes {
	public class SatanCircle : IVisualizeShape {

		public void GenerateShape( in int count, in float radius, in Vector3 parentPos, out Vector3[] posArray, out Quaternion[] rotationArray, out int[] barNums ) {
			var amountToSpawn = count * 10;
			var reverse       = true;

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

				if ( i % count == 0 ) {
					reverse = !reverse;
				}

				if ( i % count == 64 ) {
					Debug.Break();
				}

				var barNum = i % count;

				if ( reverse ) {
					barNum = count - 1 - barNum;
				}

				barNums[i] = barNum;
			}
		}
	}
}

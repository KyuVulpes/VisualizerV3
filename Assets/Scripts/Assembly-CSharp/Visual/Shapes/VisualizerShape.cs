using UnityEngine;

namespace VisualizerV3.Visual.Shapes {
	/// <summary>
	/// Used to make a consistent way to build a visualizer pattern.
	/// </summary>
	public abstract class VisualizerShape {
		protected static float Tau {
			get => Mathf.PI * 2;
		}

		/// <summary>
		/// Simple helper method to get a point on a 2D circle.
		/// </summary>
		/// <param name="radius">How big the circle is in radius.</param>
		/// <param name="theta">The angle from up.</param>
		/// <returns>A <see cref="Vector3"/> for the 3D space.</returns>
		protected static Vector3 GetXAndY( float radius, float theta ) {
			var x = radius * Mathf.Sin( theta );
			var y = radius * Mathf.Cos( theta );

			return new Vector3( x, y );
		}

		/// <summary>
		/// Used to build the shape of the visualizer.
		/// </summary>
		/// <remarks>
		/// This method is used internally to build a visualizer. You should never have to call this method.
		/// This method is meant to keep the implementation simple and not complex at all.
		///
		/// It is always best to fill up the <paramref name="barNums"/> with appropriate values, the reason it
		/// exists and needs to be filled is in cases where the bar count is bigger than <paramref name="count"/>.
		/// An example of this is when making a mirrored circle.
		/// </remarks>
		/// <param name="count">The number of visualizer bars that are needed.</param>
		/// <param name="radius">How big the circle is.</param>
		/// <param name="parentPos">Where the center is.</param>
		/// <param name="posArray">An array of positions to place the reactors.</param>
		/// <param name="rotationArray">An array of rotations to rotate the reactors.</param>
		/// <param name="barNums">An array saying what each bar number is.</param>
		public abstract void GenerateShape( in int count, in float radius, in Vector3 parentPos, out Vector3[] posArray, out Quaternion[] rotationArray, out int[] barNums );
	}
}

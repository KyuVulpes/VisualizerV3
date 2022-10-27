using UnityEngine;

namespace VisualizerV3.Audio {
	public interface IVisualizeShape {
		void GenerateShape( in int count, in float radius, in Vector3 parentPos, out Vector3[] posArray, out Quaternion[] rotationArray, out int[] barNums );
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualizerV3.Visual {
	public class Rotater : MonoBehaviour {
		[SerializeField]
		private Vector3 rotationSpeed;

		// Update is called once per frame
		private void Update() {
			transform.Rotate( rotationSpeed * Time.deltaTime, Space.Self );
		}
	}
}

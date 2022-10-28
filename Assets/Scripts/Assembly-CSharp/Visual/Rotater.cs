using UnityEngine;
using VisualizerV3.Audio;

namespace VisualizerV3.Visual {
	public class Rotater : MonoBehaviour {
		[SerializeField]
		private float baseSpeed = 1f;
		
		[SerializeField]
		private Vector3 rotationSpeed;

		[SerializeField]
		private AudioProcessor.BandFreq grab;

		private AudioProcessor processor;

		private void Start() {
			processor = AudioProcessor.MainProcessor;
		}

		// Update is called once per frame
		private void Update() {
			var increaseBy = processor.GetFreqBand( grab ) + baseSpeed;
			
			transform.Rotate( increaseBy * Time.deltaTime * rotationSpeed, Space.Self );
		}
	}
}

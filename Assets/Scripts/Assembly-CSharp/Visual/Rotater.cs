using UnityEngine;
using VisualizerV3.Audio;

namespace VisualizerV3.Visual {
	public class Rotater : MonoBehaviour {
		[SerializeField]
		private bool reactToAudio;
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
			float speed;
			
			if ( reactToAudio ) {
				speed = processor.GetFreqBand( grab ) + baseSpeed;
			} else {
				speed = 1f;
			}
			
			transform.Rotate( speed * Time.deltaTime * rotationSpeed, Space.Self );
		}
	}
}

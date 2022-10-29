using System;
using UnityEngine;
using UnityEngine.Serialization;
using VisualizerV3.Audio;

namespace VisualizerV3.Shared {
	public class Hover : MonoBehaviour {
		public enum FloatingType : byte {
			Sin = 0,
			Saw = 1,
		}

		[SerializeField]
		private bool reactToAudio;
		[SerializeField]
		private float speed = 1f;
		[SerializeField]
		private float height = 1f;
		[SerializeField]
		private float reactionDamp;
		private float time;

		[SerializeField]
		private AudioProcessor.BandFreq band;
		[FormerlySerializedAs( "floatType" )]
		[SerializeField]
		private FloatingType floatingType;

		private Vector3 basePos;
		
		private AudioProcessor processor;

		private void Awake() {
			basePos = transform.position;
		}

		private void Start() {
			processor = AudioProcessor.MainProcessor;
		}

		private void Update() {
			time += Time.deltaTime;

			var x = time * speed;

			if ( reactToAudio ) {
				x += processor.GetFreqBand( band ) / reactionDamp;
			}

			var upDist = floatingType switch {
				FloatingType.Saw => 2 / Mathf.PI * Mathf.Asin( Mathf.Sin( Mathf.PI * x ) ) * height,
				FloatingType.Sin => Mathf.Sin( x ) * height,
				_                => throw new ArgumentOutOfRangeException( nameof( floatingType ), floatingType, "Expected the floating type to be known." )
			};

			var offset = new Vector3( 0f, upDist );

			transform.position = basePos + offset;
		}
	}
}

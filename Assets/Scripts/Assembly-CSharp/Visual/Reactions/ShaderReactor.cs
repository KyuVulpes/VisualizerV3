using UnityEngine;
using VisualizerV3.Audio;

namespace VisualizerV3.Visual.Reactions {
	public class ShaderReactor : MonoBehaviour {

		private int? baseId, glowId;

		[SerializeField]
		private AudioProcessor.BandFreq freq;
		[SerializeField]
		private string shaderBaseColor;
		[SerializeField]
		private string shaderGlowColor;

		[SerializeField]
		[ColorUsage( false, true )]
		private Color baseColor;
		[SerializeField]
		private AnimationCurve baseCurve;
		[SerializeField]
		[ColorUsage( false, true )]
		private Color glowColor;
		[SerializeField]
		private AnimationCurve glowCurve;
		[SerializeField]
		private Material mat;
		private AudioProcessor processor;

		private void Awake() {
			if ( !string.IsNullOrWhiteSpace( shaderBaseColor ) ) {
				baseId = Shader.PropertyToID( shaderBaseColor );
			}

			if ( !string.IsNullOrWhiteSpace( shaderGlowColor ) ) {
				glowId = Shader.PropertyToID( shaderGlowColor );
			}
		}

		private void Start() {
			processor = AudioProcessor.MainProcessor;
		}

		private void Update() {
			if ( baseId != null ) {
				UpdateMaterial( ( int )baseId, baseColor, baseCurve );
			}

			if ( glowId != null ) {
				UpdateMaterial( ( int )glowId, glowColor, glowCurve );
			}
		}

		private void UpdateMaterial( int id, Color color, AnimationCurve curve ) {
			float offsetCurve;

			if ( curve == null || curve.length <= 1 ) {
				offsetCurve = processor.GetFreqBand( freq );
			} else {
				offsetCurve = curve.Evaluate( processor.GetFreqBand( freq ) );
			}

			var newColor = color * offsetCurve;

			mat.SetColor( id, newColor );
		}
	}
}

using System.Collections;
using UnityEngine;
using VisualizerV3.Audio;

namespace VisualizerV3.Visual.Reactions {
	public class BarReactor : MonoBehaviour {
		private static readonly int BAR_COLOR        = Shader.PropertyToID( "_BarColor" );
		private static readonly int GLOW_COLOR       = Shader.PropertyToID( "_GlowColor" );
		private static readonly int DISSOLVE_PERCENT = Shader.PropertyToID( "_DissolvePercent" );

		public int BandNumber { get; set; }

		private float minHeight;

		private Material          mat;
		private Coroutine         dissolveRoutine;
		private MeshRenderer      meshRenderer;
		private AudioProcessor    audioProcessor;
		private VisualizerManager visManager;

		// Update is called once per frame
		internal void React() {
			UpdateHeight();
			UpdateColor();

			void UpdateHeight() {
				var height   = audioProcessor[BandNumber] * visManager.Scale;
				var oldScale = transform.localScale;

				height = Mathf.Clamp( height, minHeight, visManager.UpperBound );

				transform.localScale = new Vector3( oldScale.x, height, oldScale.z );
			}

			void UpdateColor() {
				var time = transform.localScale.y / visManager.UpperBound;

				mat.SetColor( BAR_COLOR, visManager.GetColor( time ) );
				mat.SetColor( GLOW_COLOR, visManager.GetColor( time ) );
			}
		}

		// Use this for initialization
		private void Start() {
			audioProcessor = GetComponentInParent<AudioProcessor>();
			meshRenderer   = GetComponent<MeshRenderer>();
			visManager     = GetComponentInParent<VisualizerManager>();
			mat            = meshRenderer.material;

			visManager.IdleTimeoutReached += Dissolve;

			minHeight = VisualizerResources.CUBE.transform.localScale.y;
		}

		private void OnDestroy() => visManager.IdleTimeoutReached -= Dissolve;

		private void Dissolve( bool idle ) {
			var amount = mat.GetFloat( DISSOLVE_PERCENT );

			if ( dissolveRoutine != null ) {
				StopCoroutine( dissolveRoutine );
			}

			dissolveRoutine = StartCoroutine( idle ? DissolveAway( amount ) : DissolveIn( amount ) );
		}

		private IEnumerator DissolveAway( float amount ) {
			while ( amount > 0f ) {
				amount -= Time.deltaTime * ( 1 / visManager.DissolveSpeed );

				amount = Mathf.Clamp01( amount );

				mat.SetFloat( DISSOLVE_PERCENT, amount );

				yield return null;
			}

			// This helps performance by a lot, since there would probably be a lot of these renderers.
			meshRenderer.enabled = false;
		}

		private IEnumerator DissolveIn( float amount ) {
			// This helps performance by a lot, since there would probably be a lot of these renderers.
			meshRenderer.enabled = true;

			while ( amount < 1f ) {
				amount += Time.deltaTime * ( 1 / visManager.DissolveSpeed );

				amount = Mathf.Clamp01( amount );

				mat.SetFloat( DISSOLVE_PERCENT, amount );

				yield return null;
			}
		}
	}
}

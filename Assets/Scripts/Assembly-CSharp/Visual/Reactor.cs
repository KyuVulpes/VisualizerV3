using System.Collections;
using UnityEngine;

namespace VisualizerV3.Audio {
	public class Reactor : MonoBehaviour {
		private static readonly int BAR_COLOR        = Shader.PropertyToID( "_BarColor" );
		private static readonly int GLOW_COLOR       = Shader.PropertyToID( "_GlowColor" );
		private static readonly int DISSOLVE_PERCENT = Shader.PropertyToID( "_DissolvePercent" );

		public int BandNumber { get; set; }

		private float minHeight;

		private AudioProcessor    audioProcessor;
		private VisualizerManager visManager;
		private Material          mat;
		private Coroutine         dissolveRoutine;

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
			visManager     = GetComponentInParent<VisualizerManager>();
			mat            = GetComponent<MeshRenderer>().material;

			visManager.StartDissolveEffect += Dissolve;

			minHeight = VisualizerResources.CUBE.transform.localScale.y;
		}

		private void OnDestroy() => visManager.StartDissolveEffect -= Dissolve;

		private void Dissolve( bool away ) {
			var amount = mat.GetFloat( DISSOLVE_PERCENT );

			if ( dissolveRoutine != null ) {
				StopCoroutine( dissolveRoutine );
			}

			dissolveRoutine = StartCoroutine( away ? DissolveAway( amount ) : DissolveIn( amount ) );
		}

		private IEnumerator DissolveAway( float amount ) {
			while ( amount > 0f ) {
				amount -= Time.deltaTime * ( 1 / visManager.DissolveSpeed );

				amount = Mathf.Clamp01( amount );

				mat.SetFloat( DISSOLVE_PERCENT, amount );

				yield return null;
			}
		}

		private IEnumerator DissolveIn( float amount ) {
			while ( amount < 1f ) {
				amount += Time.deltaTime * ( 1 / visManager.DissolveSpeed );

				amount = Mathf.Clamp01( amount );

				mat.SetFloat( DISSOLVE_PERCENT, amount );

				yield return null;
			}
		}
	}
}

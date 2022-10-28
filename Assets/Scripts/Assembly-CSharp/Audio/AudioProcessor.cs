using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Serialization;

namespace VisualizerV3.Audio {
	public class AudioProcessor : MonoBehaviour {
		private const int   SAMPLE_COUNT           = 512;
		private const float DEFAULT_DECREASE_VALUE = 0.005f;

		private const string BAND_AMOUNT_KEY = "vis.bandAmount";

		public event Action<byte> BandAmountChanged;

		public byte BandAmount {
			get => bandAmount;
			private set {
				if ( bandAmount == value ) {
					return;
				}

				if ( 8 <= value ) {
					bandAmount = value;
				} else {
					throw new ArgumentOutOfRangeException( nameof( value ), value, $"`{nameof( value )}` has to be within [8, 128]." );
				}

				BandAmountChanged?.Invoke( value );
			}
		}

		public float this[ int band ] {
			get {
				try {
					return useBuffer ? bufferedFreqBands[band] : freqBands[band];
				} catch ( Exception e ) {
					Debug.LogError( $"Exception:\t{e.GetType().FullName}\nMessage:\t{e.Message}\nIndex:\t{band}" );

					throw;
				}
			}
		}

		[SerializeField]
		private bool useBuffer;

		private bool updating;

		[FormerlySerializedAs( "_bandAmount" )]
		[SerializeField, Range( 8, 128 )]
		private byte bandAmount = 128;

		[SerializeField, Range( 0.001f, 5f )]
		private float decreaseSpeed = 1.15f;

		private float[] bandDecrease;
		private float[] samples;
		private float[] freqBands;
		private float[] bufferedFreqBands;

		// Start is called before the first frame update
		[SuppressMessage( "Style", "IDE0062:Make local function 'static'", Justification = "Unity No Likes." )]
		private void Awake() {
			//Settings.SettingsRefreshed += SettingsRefreshed;

			//SettingsRefreshed();

			samples = new float[SAMPLE_COUNT];

			GenerateFloatArrays();

			AudioDeviceListener.ReceiveSpectrumData += ( @default, spectrum ) => {
				if ( !@default || updating ) {
					return;
				}

				lock ( samples ) {
					samples = spectrum;
				}
			};

			// void SettingsRefreshed() {
			// 	var settings = Settings.Singleton;
			//
			// 	if ( !settings.ContainsSetting( BAND_AMOUNT_KEY ) ) {
			// 		return;
			// 	}
			//
			// 	if ( !settings.TryGetSetting( BAND_AMOUNT_KEY, out byte bandAmount ) ) {
			// 		Debug.LogError( "Failed to retrieve band amount, setting to default." );
			// 	} else {
			// 		BandAmount = bandAmount;
			// 	}
			// }
		}

		// Update is called once per frame
		private void Update() {
			updating = true;

			GenerateBands();
			GenerateBufferBands();

			updating = false;
		}

		private void GenerateBands() {
			var lastIndex = 0;

			for ( var i = 0; i < bandAmount; ++i ) {
				var average   = 0f;
				var sampCount = GetSampleCount( i );
				var tempLast  = 0;
				var count     = 0;

				for ( var j = 0; j < sampCount + lastIndex && j < SAMPLE_COUNT; ++j ) {
					average  += samples[j];
					tempLast =  j + 1;
					++count;
				}

				average /= count;

				freqBands[i] = average < 0.01f ? 0f : average;
				lastIndex    = tempLast;
			}

			int GetSampleCount( int pos ) {
				var value = Mathf.Pow( 3f, ( int )( pos / ( bandAmount / 4f ) ) );

				value = Mathf.Clamp( value, 1f, bandAmount );

				return Mathf.RoundToInt( value );
			}
		}

		private void GenerateBufferBands() {
			for ( var i = 0; i < bandAmount; ++i ) {
				if ( bandDecrease[i] == 0f ) {
					bandDecrease[i] = DEFAULT_DECREASE_VALUE;
				}

				if ( freqBands[i] > bufferedFreqBands[i] ) {
					bufferedFreqBands[i] = freqBands[i];

					bandDecrease[i] = DEFAULT_DECREASE_VALUE;
				} else {
					bufferedFreqBands[i] -= bandDecrease[i];

					bandDecrease[i] *= decreaseSpeed;
				}

				bufferedFreqBands[i] = Mathf.Clamp( bufferedFreqBands[i], 0f, 100f );
			}
		}

		private void GenerateFloatArrays() {
			freqBands         = new float[bandAmount];
			bufferedFreqBands = new float[bandAmount];
			bandDecrease      = new float[bandAmount];
		}
	}
}

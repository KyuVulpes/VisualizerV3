using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using VisualizerV3.Settings;
using VisualizerV3.Visual.Reactions;
using VisualizerV3.Visual.Shapes;

namespace VisualizerV3.Audio {
	public class VisualizerManager : MonoBehaviour {
		private const string VISUALIZER_RING_TYPE = "RingType";
		private const string USE_HOLIDAY_KEY      = "EnableHolidayThemes";

		// ReSharper disable once MemberCanBePrivate.Global
		public bool UseHoliday { get; set; } = true;

		public float Scale {
			get => scale;
		}

		public float UpperBound {
			get => upperBound;
		}

		public float DissolveSpeed {
			get => dissolveSpeed;
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public VisualizerShape ShapeCreator {
			get => shapeCreator;
			set {
				if ( value is null ) {
					throw new ArgumentNullException( nameof( value ), "Expected value, but got null." );
				}

				if ( shapeCreator != null && value.GetType() == shapeCreator.GetType() ) {
					return;
				}

				shapeCreator = value;

				if ( Application.isEditor ) {
					Debug.Log( "Ring creator changed, telling ring to update." );
				}

				updateRing = true;
			}
		}

		public event Action<bool> IdleTimeoutReached;

		private bool isIdle;
		private bool updateRing;
		private bool ready = true;

		[SerializeField, Range( 1, 16 )]
		private float radius = 5f;

		[SerializeField, Range( 0.0125f, 16f )]
		private float scale = 15f;

		[SerializeField, Range( 1f, 128f )]
		private float upperBound = 8f;

		// ReSharper disable once StringLiteralTypo
		[FormerlySerializedAs( "disolveSpeed" )]
		[SerializeField, Range( 4f, 128f )]
		private float dissolveSpeed = 128f;

		[SerializeField, Range( 4f, 128f )]
		private float idleTimeout = 4f;

		private float idleTime;

		private VisualizerShape shapeCreator;

		private CancellationTokenSource tokenSource;

		[SerializeField, GradientUsage( true )]
		private Gradient normalGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient fourthGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient halloweenGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient holidayGradient;

		private Task             nowSetter;
		private Gradient         gradientToUse;
		private Coroutine        activeSwitch;
		private AudioProcessor   audioProcessor;
		private SettingsManager  settingsManager;
		private List<BarReactor> reactors;

		public Color GetColor( float value ) => gradientToUse.Evaluate( value );

		// Start is called before the first frame update
		private void Awake() {
			reactors        = new List<BarReactor>();
			tokenSource     = new CancellationTokenSource();
			audioProcessor  = GetComponent<AudioProcessor>();
			settingsManager = SettingsManager.Singleton;

			UpdateSettings();

			SettingsManager.SettingsChanged += UpdateSettings;

			ShapeCreator = new SatanCircle();

			audioProcessor.BandAmountChanged += _ => {
				if ( Application.isEditor ) {
					Debug.Log( "Band Amount changed, telling ring to update." );
				}

				updateRing = true;
			};

			nowSetter = new Task( () => { } );

			nowSetter.Start();

			activeSwitch = StartCoroutine( ChangeRing() );

			updateRing = false;
		}

		// Update is called once per frame
		private void Update() {
			if ( updateRing && activeSwitch is null ) {
				activeSwitch = StartCoroutine( ChangeRing() );

				updateRing = false;
			}

			if ( ready ) {
				if ( !isIdle ) {
					CheckForIdle();

					if ( idleTime >= idleTimeout ) {
						isIdle = true;

						IdleTimeoutReached?.Invoke( isIdle );
					}
				} else {
					CheckForActivity();
				}
			}

			try {
				MakeReactorsReact();
			} catch ( NullReferenceException ) {
				if ( activeSwitch != null ) {
					StopCoroutine( activeSwitch );
				}

				reactors = null;

				activeSwitch = StartCoroutine( ChangeRing() );
			}

			HolidayUpdateCheck();
		}

		private void OnDestroy() {
			tokenSource?.Cancel();

			nowSetter?.Wait();
			nowSetter?.Dispose();
		}

		private void HolidayUpdateCheck() {
			var gradient = normalGradient;
			var now      = DateTimeOffset.Now;

			if ( UseHoliday ) {
				if ( now.Month == 7 && now.Day == 4 ) {
					gradient = fourthGradient;
				} else if ( now.Month == 10 && now.Day == 31 ) {
					gradient = halloweenGradient;
				} else if ( now.Month == 12 ) {
					gradient = holidayGradient;
				}
			}

			gradientToUse = gradient;
		}

		private void MakeReactorsReact() {
			lock ( audioProcessor ) {
				foreach ( var reactor in reactors ) {
					if ( reactor is null || !reactor ) {
						continue;
					}

					try {
						reactor.React();
					} catch ( Exception e ) {
						Debug.LogException( e );
					}
				}
			}
		}

		private void CheckForActivity() {
			for ( var i = 0; i < audioProcessor.BandAmount; ++i ) {
				if ( audioProcessor[i] <= 0f ) {
					continue;
				}

				idleTime = 0f;
				isIdle   = false;

				IdleTimeoutReached?.Invoke( isIdle );
			}
		}

		private void CheckForIdle() {
			for ( var i = 0; i < audioProcessor.BandAmount; ++i ) {
				if ( audioProcessor[i] == 0f ) {
					continue;
				}

				idleTime = 0f;

				return;
			}

			idleTime += Time.deltaTime;
		}

		private void UpdateSettings() {
			if ( settingsManager.TryGetSetting( SettingsManager.MAIN_CONTAINER_NAME, VISUALIZER_RING_TYPE, out string strType ) ) {
				if ( ShapeCreator is null || ShapeCreator.GetType().FullName != strType ) {
					Debug.Log( $"{strType}\n{ShapeCreator is null}\n{StackTraceUtility.ExtractStackTrace()}" );
				}

				if ( string.IsNullOrWhiteSpace( strType ) ) {
					strType = typeof(SatanCircle).ToString();

					settingsManager.AddOrSetSetting( SettingsManager.MAIN_CONTAINER_NAME, VISUALIZER_RING_TYPE, strType );
				}

				var creatorType = Type.GetType( strType );

				if ( creatorType == null ) {
					ShapeCreator = new SatanCircle();

					settingsManager.AddOrSetSetting( SettingsManager.MAIN_CONTAINER_NAME, VISUALIZER_RING_TYPE, ShapeCreator.GetType().ToString() );
				} else {
					ShapeCreator = ( VisualizerShape )Activator.CreateInstance( creatorType );
				}
			} else if ( ShapeCreator is null ) {
				ShapeCreator = new SatanCircle();

				settingsManager.AddOrSetSetting( SettingsManager.MAIN_CONTAINER_NAME, VISUALIZER_RING_TYPE, ShapeCreator.GetType().ToString() );
			}

			if ( settingsManager.TryGetSetting( SettingsManager.MAIN_CONTAINER_NAME, USE_HOLIDAY_KEY, out bool useHoliday ) ) {
				UseHoliday = useHoliday;
			} else {
				settingsManager.AddOrSetSetting( SettingsManager.MAIN_CONTAINER_NAME, USE_HOLIDAY_KEY, true );
			}
		}

		private IEnumerator ChangeRing() {
			var scaleFactor = audioProcessor.BandAmount / 128f;
			var newScale    = Mathf.Pow( 1f / scaleFactor, 1.2f );

			// I do not understand what makes this an expensive method call.
			// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
			shapeCreator.GenerateShape( audioProcessor.BandAmount, radius, transform.position, out var posArray, out var rotArray, out var barNums );

			#if UNITY_EDITOR
			// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
			Debug.Log( StackTraceUtility.ExtractStackTrace() );
			#endif

			ready = false;

			// Checks for any unused reactors and disables them to free up CPU and GPU resources.
			if ( posArray.Length < reactors.Count ) {
				for ( var i = posArray.Length; i < reactors.Count; ++i ) {
					reactors[i].gameObject.SetActive( false );
				}
			}

			// Loops through the entire array of reactors to be used.
			for ( var i = 0; i < posArray.Length; ++i ) {
				BarReactor barReactor;
				Transform  rTrans;
				var        created = false;

				// A simple check to see if a reactor needs to be created or reused.
				if ( i >= transform.childCount ) {
					rTrans     = Instantiate( VisualizerResources.CUBE, transform ).transform;
					barReactor = rTrans.GetComponent<BarReactor>();

					created = true;
				} else {
					rTrans     = transform.GetChild( i );
					barReactor = rTrans.GetComponent<BarReactor>();
				}

				// This section is setting all the data for the reactor.
				rTrans.localPosition =  posArray[i];
				rTrans.localRotation =  rotArray[i];
				rTrans.localScale    *= newScale;

				barReactor.BandNumber = barNums[i];

				// If reactor is created, storing it for use later.
				if ( created ) {
					reactors.Add( barReactor );
				}

				var reactorGameObject = barReactor.gameObject;

				reactorGameObject.SetActive( true );
				reactorGameObject.name = $"Bar {i}.{barNums[i]}";

				// Let everything else have a turn now.
				yield return new WaitForEndOfFrame();
			}

			ready        = true;
			activeSwitch = null;
		}
	}
}

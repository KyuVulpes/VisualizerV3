using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using VisualizerV3.Visual.Shapes;

namespace VisualizerV3.Audio {
	public class VisualizerManager : MonoBehaviour {
		private const string ROTATE_SPEED_SETTING = "visualizer.rotSpeed";
		private const string VISUALIZER_RING_TYPE = "visualizer.ringType";
		private const string USE_HOLIDAY_KEY      = "visualizer.useHoliday";

		// ReSharper disable once MemberCanBePrivate.Global
		public bool UseHoliday { get; set; } = true;

		public float Scale => scale;

		public float UpperBound => upperBound;

		// ReSharper disable once MemberCanBePrivate.Global
		public int RotateSpeed {
			// ReSharper disable once UnusedMember.Global
			get => rotateSpeed.z;
			set => rotateSpeed.z = value;
		}

		public float DissolveSpeed => dissolveSpeed;

		// ReSharper disable once MemberCanBePrivate.Global
		public IVisualizeShape ShapeCreator {
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

		internal event Action<bool> StartDissolveEffect;

		private bool dissolve;
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

		private Vector3Int rotateSpeed = Vector3Int.zero;

		private IVisualizeShape shapeCreator;

		private CancellationTokenSource tokenSource;

		[SerializeField, GradientUsage( true )]
		private Gradient normalGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient fourthGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient halloweenGradient;

		[SerializeField, GradientUsage( true )]
		private Gradient holidayGradient;

		private Gradient  gradientToUse;
		private Reactor[] reactors;
		private Processor processor;
		private Coroutine activeSwitch;
		private Task      nowSetter;

		public Color GetColor( float value ) => gradientToUse.Evaluate( value );

		// Start is called before the first frame update
		private void Awake() {
			tokenSource = new CancellationTokenSource();
			processor   = GetComponent<Processor>();

			//Settings.SettingsRefreshed += UpdateSettings;

			//UpdateSettings();

			ShapeCreator = new SatanCircle();

			processor.BandAmountChanged += _ => {
				if ( Application.isEditor ) {
					Debug.Log( "Band Amount changed, telling ring to update." );
				}

				updateRing = true;
			};

			nowSetter = new Task( () => {} );

			nowSetter.Start();

			activeSwitch = StartCoroutine( ChangeRing() );

			updateRing = false;

			StartCoroutine( GrabNewTime() );

			IEnumerator GrabNewTime() {
				while ( true ) {
					var gradient = normalGradient;
					var now      = DateTimeOffset.Now;

					if ( UseHoliday ) {
						// Not switching to the switch statement since it makes it look weird.
						// ReSharper disable once ConvertIfStatementToSwitchStatement
						// ReSharper disable once ConvertIfStatementToSwitchExpression
						if ( now.Month == 7 && now.Day == 4 ) {
							gradient = fourthGradient;
						} else if ( now.Month == 10 && now.Day == 31 ) {
							gradient = halloweenGradient;
						} else if ( now.Month == 12 ) {
							gradient = holidayGradient;
						}
					}

					gradientToUse = gradient;

					yield return new WaitForSecondsRealtime( 30f );
				}

				// ReSharper disable once IteratorNeverReturns
			}
		}

		// Update is called once per frame
		private void Update() {
			if ( updateRing && activeSwitch is null ) {
				activeSwitch = StartCoroutine( ChangeRing() );

				updateRing = false;
			}

			if ( ready ) {
				if ( !dissolve ) {
					CheckForIdle();

					if ( idleTime >= idleTimeout ) {
						dissolve = true;

						StartDissolveEffect?.Invoke( dissolve );
					}
				} else {
					CheckForActivity();
				}

				transform.Rotate( (Vector3) rotateSpeed * Time.deltaTime );
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

			void MakeReactorsReact() {
				lock ( processor ) {
					foreach ( var reactor in reactors ) {
						if ( reactor is null || !reactor ) {
							continue;
						}

						try {
							reactor.React();
						} catch ( Exception e ) {
							// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
							Debug.LogWarning( $"A `{e.GetType().FullName}` exception occurred. The message is: {e.Message}" );
						}
					}
				}
			}
		}

		private void OnDestroy() {
			tokenSource?.Cancel();

			nowSetter?.Wait();
			nowSetter?.Dispose();
		}

		private void CheckForActivity() {
			for ( var i = 0; i < processor.BandAmount; ++i ) {
				if ( processor[i] <= 0f ) {
					continue;
				}

				idleTime = 0f;
				dissolve = false;

				StartDissolveEffect?.Invoke( dissolve );
			}
		}

		private void CheckForIdle() {
			for ( var i = 0; i < processor.BandAmount; ++i ) {
				if ( processor[i] == 0f ) {
					continue;
				}

				idleTime = 0f;

				return;
			}

			idleTime += Time.deltaTime;
		}

		private void UpdateSettings() {
			// if ( Settings.Singleton.TryGetSetting( ROTATE_SPEED_SETTING, out float rotSpeed ) ) {
			// 	RotateSpeed = (int) rotSpeed;
			// }
			//
			// if ( Settings.Singleton.TryGetSetting( VISUALIZER_RING_TYPE, out string strType ) ) {
			// 	var asm = typeof( IVisualizeShape ).Assembly;
			//
			// 	if ( ShapeCreator is null || ShapeCreator.GetType().FullName != strType ) {
			// 		Debug.Log( $"{strType}\n{ShapeCreator is null}\n{StackTraceUtility.ExtractStackTrace()}" );
			// 	}
			//
			// 	Assert.IsNotNull( strType );
			//
			// 	ShapeCreator = Activator.CreateInstance( asm.FullName, strType ).Unwrap() as IVisualizeShape;
			// } else if ( ShapeCreator is null ) {
			// 	ShapeCreator = new MirroredCircle();
			//
			// 	Debug.Log( ShapeCreator.GetType().FullName );
			// }
			//
			// if ( Settings.Singleton.TryGetSetting( USE_HOLIDAY_KEY, out bool useHoliday ) ) {
			// 	UseHoliday = useHoliday;
			// }
		}

		private IEnumerator ChangeRing() {
			var scaleFactor = processor.BandAmount / 128f;
			var newScale    = Mathf.Pow( 1f / scaleFactor, 1.2f );

			// I do not understand what makes this an expensive method call.
			// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
			shapeCreator.GenerateShape( processor.BandAmount, radius, transform.position, out var posArray, out var rotArray, out var barNums );

			#if UNITY_EDITOR
			// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
			Debug.Log( StackTraceUtility.ExtractStackTrace() );
			#endif

			ready = false;

			reactors = new Reactor[posArray.Length];

			for ( var i = 0; i < transform.childCount; ++i ) {
				Destroy( transform.GetChild( i ).gameObject );
			}

			for ( var i = 0; i < posArray.Length; ++i ) {
				// No way around it since that `GetComponent` method is needed.
				// The only other option is pooling, but that adds unnecessary complexity because there is an unknown amount of cubes needed.
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				var reactor    = Instantiate( VisualizerResources.CUBE, transform ).GetComponent<Reactor>();
				var rTransform = reactor.transform;
				
				rTransform.localPosition =  posArray[i];
				rTransform.localRotation =  rotArray[i];
				rTransform.localScale    *= newScale;

				reactor.BandNumber = barNums[i];

				reactors[i] = reactor;

				yield return new WaitForEndOfFrame();
			}

			ready = true;

			activeSwitch = null;
		}
	}
}

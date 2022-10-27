using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using UnityEngine;

namespace VisualizerV3.Audio {
	public class AudioDeviceListener {
		public static event Action<bool, float[]> ReceiveSpectrumData;

		private static int mmDeviceCount;

		private static LinkedList<AudioDeviceListener> singletons = new LinkedList<AudioDeviceListener>();

		static AudioDeviceListener() {
			var       deviceEnum       = new MMDeviceEnumerator();
			using var deviceCollection = deviceEnum.EnumAudioEndpoints( DataFlow.Render, DeviceState.Active );

			foreach ( var mmDevice in deviceCollection ) {
				AudioDeviceListener audioDeviceListener;

				try {
					audioDeviceListener = new AudioDeviceListener( mmDevice );
				} catch ( CoreAudioAPIException e ) {
					continue;
				} finally {
					++mmDeviceCount;
				}

				singletons.AddLast( audioDeviceListener );

				audioDeviceListener.IsDefaultOut = mmDevice.FriendlyName.Contains( "VAIO3" );
			}

			Application.quitting += Quitting;

			// new Task( CheckForDefaultChange ).FireAndForget( CheckDefaultFaulted );
			// new Task( CheckForDeviceChange ).FireAndForget( CheckDeviceChangeFaulted );
		}

		private static void Quitting() {
			var node = singletons.First;

			while ( node != null ) {
				try {
					node.Value?.StopListening();
				} catch ( ObjectDisposedException e ) {
					Debug.LogException( e );
				}

				singletons.Remove( node );

				node = node.Next;
			}
		}

		/// <summary>
		/// Checks to see if any new audio devices have been added or removed from the system.
		/// </summary>
		[SuppressMessage( "ReSharper", "FunctionNeverReturns", Justification = "Method is meant to be looping forever." )]
		private static void CheckForDeviceChange() {
			while ( true ) {
				Task.Delay( 128 ).Wait();

				using var deviceEnum       = new MMDeviceEnumerator();
				using var deviceCollection = deviceEnum.EnumAudioEndpoints( DataFlow.Render, DeviceState.Active );

				lock ( singletons ) {
					if ( deviceCollection.Count == mmDeviceCount ) {
						continue;
					}

					var newListeners = new LinkedList<AudioDeviceListener>();

					foreach ( var mmDevice in deviceCollection ) {
						var listenerPresent = false;

						foreach ( var listener in singletons.Where( listener => listener.OutDevice.DeviceID == mmDevice.DeviceID ) ) {
							listenerPresent = true;

							newListeners.AddLast( listener );

							break;
						}

						if ( listenerPresent ) {
							Debug.Log( $"Listener for device `{mmDevice.DeviceID}/{mmDevice.FriendlyName}` doesn't need to die." );

							continue;
						}

						Debug.Log( $"Creating new listener for `{mmDevice.DeviceID}/{mmDevice.FriendlyName}` since one does not exist." );

						AudioDeviceListener newAudioDeviceListener;

						try {
							newAudioDeviceListener = new AudioDeviceListener( mmDevice );
						} catch ( CoreAudioAPIException e ) {
							continue;
						} finally {
							++mmDeviceCount;
						}

						newListeners.AddLast( newAudioDeviceListener );
					}

					singletons = newListeners;
				}
			}
		}

		/// <summary>
		/// Looks to see if the default audio output has changed.
		/// </summary>
		// [SuppressMessage( "ReSharper", "FunctionNeverReturns", Justification = "Method is meant to be looping forever." )]
		// private static void CheckForDefaultChange() {
		// 	while ( true ) {
		// 		Task.Delay( 256 ).Wait();
		//
		// 		var alreadyFoundOut = false;
		//
		// 		if ( Settings.Singleton.TryGetSetting( OVERRIDE_DEFAULT_LISTEN_DEVICE_KEY, out string device ) ) {
		// 			lock ( singletons ) {
		// 				foreach ( var listener in singletons ) {
		// 					if ( alreadyFoundOut || !listener.OutDevice.FriendlyName.Equals( device ) ) {
		// 						listener.IsDefaultOut = false;
		//
		// 						continue;
		// 					}
		//
		// 					listener.IsDefaultOut = true;
		// 					alreadyFoundOut       = true;
		// 				}
		// 			}
		//
		// 			continue;
		// 		}
		//
		// 		using var deviceEnum    = new MMDeviceEnumerator();
		// 		var       defaultDevice = deviceEnum.GetDefaultAudioEndpoint( DataFlow.Render, Role.Multimedia );
		//
		// 		lock ( singletons ) {
		// 			foreach ( var listener in singletons ) {
		// 				if ( alreadyFoundOut || !listener.OutDevice.DeviceID.Equals( defaultDevice.DeviceID, StringComparison.OrdinalIgnoreCase ) ) {
		// 					listener.IsDefaultOut = false;
		//
		// 					continue;
		// 				}
		//
		// 				listener.IsDefaultOut = true;
		// 				alreadyFoundOut       = true;
		// 			}
		// 		}
		// 	}
		// }

		private bool IsDefaultOut { get; set; }

		private MMDevice OutDevice => loopback.Device;

		private float[] buffer;

		private readonly WasapiLoopbackCapture         loopback = new WasapiLoopbackCapture();
		private          SoundInSource                 soundIn;
		private          SingleBlockNotificationStream blockNotifyStream;
		private          BasicSpectrumProvider         spectrumProvider;
		private          LineSpectrum                  spectrum;

		private IWaveSource realtime;

		private AudioDeviceListener( MMDevice device ) {
			loopback.Device = device;

			CreateLoopback();
		}

		private void StopListening() {
			if ( blockNotifyStream != null ) {
				blockNotifyStream.SingleBlockRead -= SingleBlockRead;
			}

			soundIn?.Dispose();
			realtime?.Dispose();

			loopback?.Stop();
			loopback?.Dispose();
		}

		[SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "It seems to be weird, so it stays." )]
		private void CreateLoopback() {
			loopback.Initialize();

			soundIn          = new SoundInSource( loopback );
			spectrumProvider = new BasicSpectrumProvider( soundIn.WaveFormat.Channels, soundIn.WaveFormat.SampleRate, FftSize.Fft4096 );
			spectrum = new LineSpectrum( FftSize.Fft4096 ) {
				SpectrumProvider = spectrumProvider,
				BarCount         = 512,
				UseAverage       = true,
				IsXLogScale      = true,
			};

			loopback.Start();

			blockNotifyStream = new SingleBlockNotificationStream( soundIn.ToSampleSource() );
			realtime          = blockNotifyStream.ToWaveSource();

			buffer = new float[realtime.WaveFormat.BytesPerSecond / sizeof( float ) / 2];

			soundIn.DataAvailable += AudioDataAvailable;

			blockNotifyStream.SingleBlockRead += SingleBlockRead;
		}

		private void SingleBlockRead( object sender, SingleBlockReadEventArgs e ) => spectrumProvider.Add( e.Left, e.Right );

		private void AudioDataAvailable( object sender, DataAvailableEventArgs e ) {
			var byteBuffer = new byte[buffer.Length * sizeof( float )];

			while ( realtime.Read( byteBuffer, 0, byteBuffer.Length ) > 0 ) {
				var spectrumData = spectrum.GetSpectrumData( 10 );

				if ( spectrumData != null ) {
					ReceiveSpectrumData?.Invoke( IsDefaultOut, spectrumData );
				}
			}
		}
	}
}

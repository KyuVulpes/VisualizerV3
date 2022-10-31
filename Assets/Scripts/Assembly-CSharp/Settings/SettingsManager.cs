using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace VisualizerV3.Settings {
	public class SettingsManager {

		#region Class

		internal const string MAIN_CONTAINER_NAME = "Core";

		public static event Action SettingsChanged;

		public string SettingsLocation {
			get => Application.persistentDataPath;
		}

		public string MainSaveFile {
			get => Path.Combine( SettingsLocation, "settings.json" );
		}

		public static SettingsManager Singleton {
			get => singleton ??= new SettingsManager();
		}

		private static SettingsManager singleton;

		#endregion

		private byte[] checksum = new byte[32];

		private readonly JsonSerializer serializer;

		private Dictionary<string, SettingsContainer> settings;

		private SettingsManager() {
			serializer = new JsonSerializer {
				Formatting                 = Formatting.Indented,
				ConstructorHandling        = ConstructorHandling.AllowNonPublicDefaultConstructor,
				DateFormatHandling         = DateFormatHandling.IsoDateFormat,
				DateParseHandling          = DateParseHandling.DateTimeOffset,
				FloatFormatHandling        = FloatFormatHandling.Symbol,
				FloatParseHandling         = FloatParseHandling.Double,
				NullValueHandling          = NullValueHandling.Include,
				DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
				MissingMemberHandling      = MissingMemberHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.All,
				ReferenceLoopHandling      = ReferenceLoopHandling.Ignore,
				ObjectCreationHandling     = ObjectCreationHandling.Auto,
				StringEscapeHandling       = StringEscapeHandling.EscapeNonAscii,
				DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
			};

			if ( File.Exists( MainSaveFile ) ) {
				LoadSettingFile();
			} else {
				settings = new Dictionary<string, SettingsContainer>();
			}

			var settingsCheck = new FileSystemWatcher( SettingsLocation ) {
				EnableRaisingEvents = true,
				Filter = "settings.json",
				Path = SettingsLocation,
			};

			settingsCheck.Changed += ( _, _ ) => {
				using var fStream      = new FileStream( MainSaveFile, FileMode.Open, FileAccess.Read, FileShare.Read );
				using var sha256       = new SHA256Managed();
				var       calcChecksum = sha256.ComputeHash( fStream );

				for ( var i = 0; i < 32; ++i ) {
					if ( calcChecksum[i] == this.checksum[i] ) {
						continue;
					}

					SettingsChanged?.Invoke();

					checksum = calcChecksum;

					return;
				}
			};
		}

		public object GetSetting( string containerPath, string key ) {
			var container = settings[containerPath];

			return container.GetSetting( key );
		}

		public void SetSetting( string containerPath, string key, object value ) {
			// Remove the line below to support this method.
			// throw new NotSupportedException( "This method isn't supported in this build of the program." );

			var container = settings[containerPath];

			container.SetSetting( key, value );
		}

		public void AddOrSetSetting( string containerPath, string key, object value ) {
			// Remove the line below to support this method.
			// throw new NotSupportedException( "This method isn't supported in this build of the program." );

			SettingsContainer container;

			if ( !settings.ContainsKey( containerPath ) ) {
				container = new SettingsContainer( new Dictionary<string, object>() );

				settings.Add( containerPath, container );
			} else {
				container = settings[containerPath];
			}

			container.AddSetting( key, value );
		}

		public bool TryGetSetting<T>( string containerPath, string key, out T value ) {
			SettingsContainer container;

			value = default;
			
			try {
				container = settings[containerPath];
			} catch ( Exception ) {
				return false;
			}

			return container.TryGetSetting( key, out value );
		}

		private void LoadSettingFile() {
			using var fStream    = new FileStream( MainSaveFile, FileMode.Open, FileAccess.Read );
			using var reader     = new StreamReader( fStream, Encoding.Unicode );
			using var jsonReader = new JsonTextReader( reader );

			settings = serializer.Deserialize<Dictionary<string, SettingsContainer>>( jsonReader );
		}
	}
}

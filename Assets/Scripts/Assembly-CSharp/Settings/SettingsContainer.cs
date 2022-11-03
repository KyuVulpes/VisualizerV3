using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VisualizerV3.Settings {
	internal sealed class SettingsContainer {
		
		[JsonExtensionData]
		private readonly Dictionary<string, object> containedSettings;

		public SettingsContainer( Dictionary<string, object> containedSettings ) {
			this.containedSettings = containedSettings ?? new Dictionary<string, object>();
		}

		public bool ContainsKey( string key ) => containedSettings.ContainsKey( key );

		public bool ContainsValue( object value ) => containedSettings.ContainsValue( value );

		public object GetSetting( string key ) => containedSettings[key];

		public void SetSetting( string key, object value ) => containedSettings[key] = value;

		public void AddSetting( string key, object value ) => containedSettings.Add( key, value );

		public bool TryGetSetting( string key, out object value ) {
			value = null;

			if ( !ContainsKey( key ) ) {
				return false;
			}

			value = GetSetting( key );

			return true;
		}

		public void AddOrSetSetting( string key, object value ) {
			if ( ContainsKey( key ) ) {
				SetSetting( key, value );
			} else {
				AddSetting( key, value );
			}
		}

		public bool TryGetSetting<T>( string key, out T value ) {
			value = default;

			if ( !TryGetSetting( key, out var tmpObject ) ) {
				return false;
			}

			try {
				value = ( T )Convert.ChangeType( tmpObject, typeof(T) );
			} catch {
				return false;
			}

			return true;
		}
	}
}

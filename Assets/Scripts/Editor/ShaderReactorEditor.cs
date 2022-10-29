using System;
using UnityEditor;
using UnityEngine;
using VisualizerV3.Audio;
using VisualizerV3.Visual.Reactions;

namespace VisualizerV3.Editor {
	[CustomEditor( typeof(ShaderReactor) )]
	[CanEditMultipleObjects]
	public class ShaderReactorEditor : UnityEditor.Editor {
		private SerializedProperty freq;
		private SerializedProperty material;
		// Base Color of the Shader
		private SerializedProperty shaderBaseKey;
		private SerializedProperty shaderBaseColor;
		private SerializedProperty shaderBaseCurve;
		// Emission of the shader.
		private SerializedProperty shaderGlowKey;
		private SerializedProperty shaderGlowColor;
		private SerializedProperty shaderGlowCurve;

		private void OnEnable() {
			freq     = serializedObject.FindProperty( "freq" );
			material = serializedObject.FindProperty( "mat" );
			// Base
			shaderBaseKey   = serializedObject.FindProperty( "shaderBaseColor" );
			shaderBaseColor = serializedObject.FindProperty( "baseColor" );
			shaderBaseCurve = serializedObject.FindProperty( "baseCurve" );
			// Emission
			shaderGlowKey   = serializedObject.FindProperty( "shaderGlowColor" );
			shaderGlowColor = serializedObject.FindProperty( "glowColor" );
			shaderGlowCurve = serializedObject.FindProperty( "glowCurve" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			freq.enumValueFlag = ( byte )( AudioProcessor.BandFreq )EditorGUILayout.EnumPopup( "Band", ( AudioProcessor.BandFreq )Enum.GetValues( typeof(AudioProcessor.BandFreq) ).GetValue( freq.enumValueIndex ) );

			EditorGUILayout.ObjectField( material, new GUIContent( "Material" ) );

			shaderBaseKey.stringValue = EditorGUILayout.TextField( "Base Color Key", shaderBaseKey.stringValue );

			if ( !string.IsNullOrWhiteSpace( shaderBaseKey.stringValue ) ) {
				EditorGUILayout.PropertyField( shaderBaseColor );
				EditorGUILayout.PropertyField( shaderBaseCurve );
				EditorGUILayout.HelpBox( "Please note that values can get as high as 3 in some instances, please take that into account when setting a curve.", MessageType.Info );
				EditorGUILayout.Separator();
			}

			shaderGlowKey.stringValue = EditorGUILayout.TextField( "Emission Key", shaderGlowKey.stringValue );

			if ( !string.IsNullOrWhiteSpace( shaderGlowKey.stringValue ) ) {
				EditorGUILayout.PropertyField( shaderGlowColor );
				EditorGUILayout.PropertyField( shaderGlowCurve );
				EditorGUILayout.HelpBox( "Please note that values can get as high as 3 in some instances, please take that into account when setting a curve.", MessageType.Info );
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}

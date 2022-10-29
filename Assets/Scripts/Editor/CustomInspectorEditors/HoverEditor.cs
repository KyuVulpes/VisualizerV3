using System;
using UnityEditor;
using VisualizerV3.Audio;
using VisualizerV3.Shared;

namespace VisualizerV3.Editor {
	[CustomEditor( typeof(Hover) )]
	[CanEditMultipleObjects]
	public class HoverEditor : UnityEditor.Editor {
		private float falseHeight;

		private SerializedProperty react;
		private SerializedProperty speed;
		private SerializedProperty height;
		private SerializedProperty reactDamp;
		private SerializedProperty band;
		private SerializedProperty floatingType;

		private void OnEnable() {
			react        = serializedObject.FindProperty( "reactToAudio" );
			speed        = serializedObject.FindProperty( "speed" );
			height       = serializedObject.FindProperty( "height" );
			reactDamp    = serializedObject.FindProperty( "reactionDamp" );
			band         = serializedObject.FindProperty( "band" );
			floatingType = serializedObject.FindProperty( "floatingType" );

			falseHeight = height.floatValue / 2f;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField( speed );

			falseHeight = EditorGUILayout.Slider( "Height", falseHeight, 0.25f, 5f );
			
			floatingType.enumValueIndex = ( byte )( Hover.FloatingType )EditorGUILayout.EnumPopup( "Band", ( Hover.FloatingType )Enum.GetValues( typeof(Hover.FloatingType) ).GetValue( floatingType.enumValueIndex ) );
			
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField( react );

			if ( react.boolValue ) {
				reactDamp.floatValue = EditorGUILayout.Slider( "Dampen Audio Value", reactDamp.floatValue, 10f, 30f );
				band.enumValueIndex  = ( byte )( AudioProcessor.BandFreq )EditorGUILayout.EnumPopup( "Band", ( AudioProcessor.BandFreq )Enum.GetValues( typeof(AudioProcessor.BandFreq) ).GetValue( band.enumValueIndex ) );
			}

			height.floatValue = falseHeight * 2;

			if ( speed.floatValue < 1f ) {
				speed.floatValue = 1f;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}

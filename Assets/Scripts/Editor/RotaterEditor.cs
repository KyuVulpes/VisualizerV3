using System;
using UnityEditor;
using VisualizerV3.Visual;

namespace VisualizerV3.Editor {
	[CustomEditor(typeof(Rotater))]
	[CanEditMultipleObjects]
	public class RotaterEditor : UnityEditor.Editor {

		private SerializedProperty react;
		private SerializedProperty baseSpeed;
		private SerializedProperty rotSpeed;
		private SerializedProperty bandFreq;

		private void OnEnable() {
			react     = serializedObject.FindProperty( "reactToAudio" );
			baseSpeed = serializedObject.FindProperty( "baseSpeed" );
			rotSpeed  = serializedObject.FindProperty( "rotationSpeed" );
			bandFreq  = serializedObject.FindProperty( "grab" );
		}


		public override void OnInspectorGUI() {
			var reactToAudio = react.boolValue;
			var rotVector    = rotSpeed.vector3Value;
			
			serializedObject.Update();

			reactToAudio = EditorGUILayout.Toggle( "React To Audio", reactToAudio );

			rotVector = EditorGUILayout.Vector3Field( reactToAudio ? "Baseline Rotation Speed" : "Base Speed", rotVector );
			
			EditorGUILayout.Separator();

			if ( reactToAudio ) {
				ShowMoreOptions();
			} else {
				baseSpeed.floatValue = 1f;
			}

			react.boolValue       = reactToAudio;
			rotSpeed.vector3Value = rotVector;

			serializedObject.ApplyModifiedProperties();
		}

		private void ShowMoreOptions() {
			EditorGUILayout.PropertyField( baseSpeed );
			EditorGUILayout.PropertyField( bandFreq );
		}
	}
}

using System;
using UnityEditor;
using VisualizerV3.Audio;

namespace VisualizerV3.Editor {
	[CustomEditor( typeof(AudioProcessor) )]
	[CanEditMultipleObjects]
	public class AudioProcessorEditor : UnityEditor.Editor {
		private bool isFreqRangeShown;
		
		private SerializedProperty separator;
		private SerializedProperty decreaseSpeed;
		private SerializedProperty bandAmount;
		private SerializedProperty useBuffer;

		private void OnEnable() {
			separator     = serializedObject.FindProperty( "separators" );
			decreaseSpeed = serializedObject.FindProperty( "decreaseSpeed" );
			bandAmount    = serializedObject.FindProperty( "bandAmount" );
			useBuffer     = serializedObject.FindProperty( "useBuffer" );
		}

		public override void OnInspectorGUI() {

			serializedObject.Update();
			EditorGUILayout.PropertyField( useBuffer );
			EditorGUILayout.PropertyField( bandAmount );
			EditorGUILayout.PropertyField( decreaseSpeed );

			EditorGUILayout.Separator();
			isFreqRangeShown = EditorGUILayout.BeginFoldoutHeaderGroup( isFreqRangeShown, "Frequency Bands Coverage" );

			if ( isFreqRangeShown ) {
				ShowFreqRange();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void ShowFreqRange() {
			var sepVal = separator.vector3Value;
			
			sepVal.x = EditorGUILayout.Slider( "Base Percentage", sepVal.x, 0f, 1f );

			var remainder = 1f - sepVal.x;
			
			sepVal.y = EditorGUILayout.Slider( "Mids Percentage", sepVal.y, 0f, remainder );

			remainder -= sepVal.y;
			
			sepVal.z = EditorGUILayout.Slider( "Upper Mids Percentage", sepVal.z, 0f, remainder );

			remainder -= sepVal.z;

			if ( remainder < 0f ) {
				EditorGUILayout.HelpBox( "Cannot have a freq range cover less than 0%.", MessageType.Error );
			} else {
				EditorGUILayout.LabelField( $"Highs", remainder.ToString( "F2" ) );
			}

			separator.vector3Value = sepVal;
		}
	}
}

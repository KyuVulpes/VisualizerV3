using UnityEditor;
using UnityEngine;
using VisualizerV3.Information;

namespace VisualizerV3.Editor {
	[CustomEditor( typeof(ObjectData) )]
	[CanEditMultipleObjects]
	public class ObjectDataEditor : UnityEditor.Editor {

		private SerializedProperty author;
		private SerializedProperty projectName;
		private SerializedProperty version;
		private SerializedProperty copyright;
		private SerializedProperty icon;

		private void OnEnable() {
			author      = serializedObject.FindProperty( "author" );
			version     = serializedObject.FindProperty( "version" );
			copyright   = serializedObject.FindProperty( "copyright" );
			icon        = serializedObject.FindProperty( "icon" );
			projectName = serializedObject.FindProperty( "projectName" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			projectName.stringValue   = EditorGUILayout.TextField( "Project Name", projectName.stringValue );
			author.stringValue        = EditorGUILayout.TextField( "Author Info", author.stringValue );
			version.stringValue       = EditorGUILayout.TextField( "Version", version.stringValue );
			icon.objectReferenceValue = EditorGUILayout.ObjectField( "Select Icon", icon.objectReferenceValue, typeof(Texture2D), true );

			EditorGUILayout.Separator();
			EditorGUILayout.PrefixLabel( "Copyright:" );

			copyright.stringValue = EditorGUILayout.TextArea( copyright.stringValue );

			serializedObject.ApplyModifiedProperties();
		}
	}
}

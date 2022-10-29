using System;
using UnityEditor;
using UnityEngine;
using VisualizerV3.Information;

namespace VisualizerV3.Editor {
	[CustomEditor( typeof(ObjectData) )]
	[CanEditMultipleObjects]
	public class ObjectDataEditor : UnityEditor.Editor {

		private SerializedProperty author;
		private SerializedProperty version;
		private SerializedProperty copyright;
		private SerializedProperty icon;

		private void OnEnable() {
			author    = serializedObject.FindProperty( "author" );
			version   = serializedObject.FindProperty( "version" );
			copyright = serializedObject.FindProperty( "copyright" );
			icon      = serializedObject.FindProperty( "icon" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			author.stringValue    = EditorGUILayout.TextField( "Author Info", author.stringValue );
			version.stringValue   = EditorGUILayout.TextField( "Version", version.stringValue );
			copyright.stringValue = EditorGUILayout.TextField( "Copyright", copyright.stringValue );

			var rect = EditorGUILayout.BeginHorizontal();

			icon.objectReferenceValue = EditorGUILayout.ObjectField( "Select Icon", icon.objectReferenceValue, typeof(Texture2D), true );

			rect.height = 256;
			
			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}
	}
}

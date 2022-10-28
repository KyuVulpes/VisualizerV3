using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public class ImportRotations : EditorWindow {

	private static ImportRotations window;

	[MenuItem("Window/Import Rotations")]
	public static void ShowWindow() {
		if ( window == null ) {
			window = CreateInstance<ImportRotations>();
		}

		window.Show();
	}

	private string filePath;

	private Transform root;

	private void OnGUI() {
		filePath = EditorGUILayout.TextField( "Rotation File", filePath );

		if ( GUILayout.Button( "Select File" ) ) {
			filePath = EditorUtility.OpenFilePanel( "Open Rotation File", Application.dataPath, "rot" );
		}

		if ( root != null && GUILayout.Button( "Import" ) && !string.IsNullOrWhiteSpace( filePath ) ) {
			LoadData();
		}

		root = EditorGUILayout.ObjectField( root, typeof(Transform), true ) as Transform;
	}

	private void LoadData() {
		Undo.IncrementCurrentGroup();
		
		using ( var fStream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) ) {
			using ( var reader = new BinaryReader( fStream, Encoding.UTF8 ) ) {
				ParseData( reader );
			}
		}
	}

	private void ParseData( BinaryReader reader ) {
		var rootCounts = reader.ReadInt32();

		for ( var i = 0; i < rootCounts; ++i ) {
			var rootName      = reader.ReadString();
			var rootTransform = root.name == rootName ? root : root.Find( rootName );
			var rot           = ParseRotationData( reader );
			var childCount    = reader.ReadInt32();
			
			Undo.RecordObject( rootTransform, $"Changing rotation {rootName}" );

			rootTransform.rotation = rot;

			for ( var j = 0; j < childCount; ++j ) {
				var name      = reader.ReadString();
				var transform = rootTransform.Find( name );
				
				Undo.RecordObject( transform, $"Changing transform rotation {name}" );

				transform.rotation = ParseRotationData( reader );
			}
		}
	}

	private static Quaternion ParseRotationData( BinaryReader reader ) {
		var w = reader.ReadSingle();
		var x = reader.ReadSingle();
		var y = reader.ReadSingle();
		var z = reader.ReadSingle();

		return new Quaternion( x, y, z, w );
	}

}

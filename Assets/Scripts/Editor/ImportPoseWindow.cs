using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace VisualizerV3.Editor {
	public class ImportPoseWindow : EditorWindow {
		private class TransformData : IEnumerable<TransformData> {

			public Vector3 Position {
				get;
				set;
			}

			public Vector3 Scale {
				get;
				set;
			}

			public Quaternion Rotation {
				get;
				set;
			}

			public string GameObjectName {
				get;
				set;
			}

			public TransformData this[ int i ] {
				get => transformDatas[i];
				set {
					if ( i >= transformDatas.Count ) {
						transformDatas.Add( value );
					} else {
						transformDatas[i] = value;
					}
				}
			}

			[JsonRequired]
			[JsonProperty]
			private List<TransformData> transformDatas;

			public TransformData() {
				transformDatas = new List<TransformData>();
			}

			public TransformData( Transform transform ) : this( transform.localPosition, transform.localScale, transform.localRotation, transform.gameObject.name ) {

			}

			public TransformData( Vector3 pos, Vector3 scale, Quaternion rot, string name ) : this() {
				Position       = pos;
				Scale          = scale;
				Rotation       = rot;
				GameObjectName = name;
			}

			public IEnumerator<TransformData> GetEnumerator() => transformDatas.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}

		private static ImportPoseWindow window;

		public static void ShowWindow() {
			if ( window == null ) {
				window = CreateInstance<ImportPoseWindow>();
			}

			window.Show();
		}

		private static void ApplyData( Transform root, TransformData data ) {
			root.localPosition = data.Position;
			root.localScale    = data.Scale;
			root.localRotation = data.Rotation;

			foreach ( var childData in data ) {
				var child = root.Find( childData.GameObjectName );

				if ( child == null ) {
					continue;
				}

				ApplyData( child, childData );
			}
		}

		private Transform root;

		private void OnGUI() {
			root = EditorGUILayout.ObjectField( root, typeof(Transform), true ) as Transform;

			if ( GUILayout.Button( "Import Pose" ) ) {
				ImportPose();
			}
		}

		private void ImportPose() {
			var file          = EditorUtility.OpenFilePanel( "Open Pose File", Application.dataPath, "json" );
			var json          = File.ReadAllText( file );
			var transformData = JsonConvert.DeserializeObject<TransformData>( json );

			ApplyData( root, transformData );
		}
	}
}

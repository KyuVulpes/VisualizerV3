using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoneTransfer : EditorWindow {
	private struct BonePosition {
		public int       bonePos;
		public Transform bone;
	}

	private static BoneTransfer window;

	[MenuItem("Window/Bone Transfer")]
	public static void ShowWindow() {
		if ( window == null ) {
			window = CreateInstance<BoneTransfer>();
		}

		window.Show();
	}

	private int AvatarRendererId => avatarRenderer == null ? 0 : avatarRenderer.GetInstanceID();
	
	private int AccessoryRendererId => accessoryRenderer == null ? 0 : accessoryRenderer.GetInstanceID();

	[SerializeField]
	private Vector2 scrollPos;

	[SerializeField]
	private SkinnedMeshRenderer avatarRenderer;
	[SerializeField]
	private SkinnedMeshRenderer accessoryRenderer;
	[SerializeField]
	private Transform[] avatarBones = Array.Empty<Transform>();
	[SerializeField]
	private Transform[] accessoryBones = Array.Empty<Transform>();

	private void OnGUI() {
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		if ( avatarRenderer != null && accessoryRenderer != null && GUILayout.Button("Apply Changes") ) {
			accessoryRenderer.bones = accessoryBones;
			accessoryBones = new Transform[accessoryBones.Length];

			Array.Copy(accessoryRenderer.bones, accessoryBones, accessoryBones.Length);
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		EditorGUILayout.PrefixLabel("Avatar:");

		DrawBoneList(ref avatarRenderer, ref avatarBones, AvatarRendererId);

		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		EditorGUILayout.PrefixLabel("Accessory:");

		DrawBoneList(ref accessoryRenderer, ref accessoryBones, AccessoryRendererId);

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}

	private void DrawBoneList( ref SkinnedMeshRenderer renderer, ref Transform[] bones, int currentId ) {
		renderer = EditorGUILayout.ObjectField(renderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

		if ( renderer == null ) {
			bones = Array.Empty<Transform>();
		} else if ( renderer.GetInstanceID() != currentId ) {
			bones = new Transform[renderer.bones.Length];

			Array.Copy(renderer.bones, bones, bones.Length);
		}

		for ( var i = 0; i < bones.Length; ++i ) {
			bones[i] = EditorGUILayout.ObjectField(bones[i], typeof(Transform), true) as Transform;
		}
	}

	// private void DrawColumn( SkinnedMeshRenderer columnMesh ) {
	// 	columnMesh = EditorGUILayout.ObjectField( columnMesh, typeof(SkinnedMeshRenderer), true ) as SkinnedMeshRenderer;
	//
	// 	if ( columnMesh == null ) {
	// 		return;
	// 	}
	//
	// 	foreach ( var bone in columnMesh.bones ) {
	// 		_ = EditorGUILayout.ObjectField( bone, typeof(Transform), true ) as Transform;
	// 	}
	// }
}

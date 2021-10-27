using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FriendSea
{
	[CustomEditor(typeof(StateMachineImporter))]
	public class StateMachineImporterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if(GUILayout.Button("Open Graph Editor"))
			{
				StateMachineGraphWindow.Open(target as StateMachineImporter);
			}
			using (new EditorGUI.DisabledGroupScope(true))
				base.OnInspectorGUI();
		}
	}
}

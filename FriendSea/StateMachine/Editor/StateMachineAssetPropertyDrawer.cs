using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FriendSea.StateMachine
{
    [CustomPropertyDrawer(typeof(StateMachineAsset))]
    public class StateMachineAssetPropertyDrawer : PropertyDrawer
    {
		const float newButtonWidth = 50f;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.width -= newButtonWidth;
			EditorGUI.PropertyField(position, property, label);
			position.x += position.width;
			position.width = newButtonWidth;
			if(GUI.Button(position, "New"))
			{
				var path = EditorUtility.SaveFilePanelInProject("Create StateMachineAsset", "NewStateMachine", "friendseastatemachine", "Select the destination to save new StateMachineAsset.");
				if(!string.IsNullOrEmpty(path))
				{
					File.WriteAllText(path, StateMachineImporter.GetDefaultText());
					AssetDatabase.Refresh();
					property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<StateMachineAsset>(path);
					property.serializedObject.ApplyModifiedProperties();
				}
				GUIUtility.ExitGUI();
			}
		}
	}
}

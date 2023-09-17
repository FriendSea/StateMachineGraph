using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("Controls/Trigger")]
	class TriggerNode : IStateMachineNode
	{
		[SerializeField]
		TriggerLabel label;

		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new Controls.Trigger() {
				label = label,
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};

		public class TriggerNodeInitializer : StateMachineNodeInitializerBase
		{
			public override Type TargetType => typeof(TriggerNode);
			public override void Initialize(GraphNode node)
			{
				node.title = "Trigger";
				node.style.width = 150f;
				SetupInputPort(node);
				SetupOutputPort(node);
				InitializeInternal(node);
				node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(TriggerNode));
			}
		}
	}

	[CustomPropertyDrawer(typeof(TriggerNode))]
	class TriggerDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
			EditorGUI.PropertyField(position, property.FindPropertyRelative("label"), GUIContent.none);
	}

	[CustomPropertyDrawer(typeof(TriggerLabel))]
	class TriggerLabelDrawer : PropertyDrawer
	{
		const float newButtonWidth = 20f;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.width -= newButtonWidth;

			var labels = AssetDatabase.FindAssets($"t:{nameof(TriggerLabel)}").Select(guid => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid))).Prepend(null).ToList();
			var currentIndex = Mathf.Max(labels.IndexOf(property.objectReferenceValue), 0);
			var newIndex = EditorGUI.Popup(position, label, currentIndex, labels.Select(o => new GUIContent(o?.name ?? "<None>")).ToArray());
			if (currentIndex != newIndex)
				property.objectReferenceValue = labels[newIndex];

			position.x += position.width;
			position.width = newButtonWidth;

			if (GUI.Button(position, "+"))
			{
				var path = EditorUtility.SaveFilePanelInProject(
					"Save Trigger Label Asset",
					"NewTrigger",
					"asset",
					"");
				if (string.IsNullOrEmpty(path)) return;
				var asset = ScriptableObject.CreateInstance<TriggerLabel>();
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.Refresh();
				property.objectReferenceValue = asset;
			}
		}
	}
}

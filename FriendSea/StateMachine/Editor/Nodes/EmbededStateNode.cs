using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("State References/Embeded State")]
	public class EmbededStateNode : IStateMachineNode
	{
		[SerializeField]
		EmbededStateLabel label;

		public State.IStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new EmbededStateReference(){ label = label };

		public class TriggerNodeInitializer : StateMachineNodeInitializerBase
		{
			public override Type TargetType => typeof(EmbededStateNode);
			public override void Initialize(GraphNode node)
			{
				node.title = "EmbededState";
				SetupInputPort(node);
				InitializeInternal(node);
				node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(TriggerNode));
			}
		}
	}

	[CustomPropertyDrawer(typeof(EmbededStateNode))]
	class EmbededStateDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
			EditorGUI.PropertyField(position, property.FindPropertyRelative("label"), GUIContent.none);
	}

	[CustomPropertyDrawer(typeof(EmbededStateLabel))]
	class EmbededStateLabelDrawer : PropertyDrawer
	{
		const float newButtonWidth = 20f;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.width -= newButtonWidth;

			var labels = AssetDatabase.FindAssets($"t:{nameof(EmbededStateLabel)}").Select(guid => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid))).Prepend(null).ToList();
			var currentIndex = Mathf.Max(labels.IndexOf(property.objectReferenceValue), 0);
			var newIndex = EditorGUI.Popup(position, label, currentIndex, labels.Select(o => new GUIContent(o?.name ?? "<None>")).ToArray());
			if (currentIndex != newIndex)
				property.objectReferenceValue = labels[newIndex];

			position.x += position.width;
			position.width = newButtonWidth;

			if (GUI.Button(position, "+"))
			{
				var path = EditorUtility.SaveFilePanelInProject(
					"Save EmbededState Label Asset",
					"NewEmbededStateLabel",
					"asset",
					"");
				if (string.IsNullOrEmpty(path)) return;
				var asset = ScriptableObject.CreateInstance<EmbededStateLabel>();
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.Refresh();
				property.objectReferenceValue = asset;
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using FriendSea.StateMachine.Controls;
using FriendSea.GraphViewSerializer;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("Transition")]
	class TransitionNode : IStateMachineNode
	{
		[SerializeReference]
		internal Transition.ICondition transition;

		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new Transition()
			{
				condition = (node.data as TransitionNode).transition,
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};
	}

	class TransitionNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(TransitionNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Transition";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMachineGraphSettings.GetColor(typeof(TransitionNode));
		}
	}

	[CustomPropertyDrawer(typeof(TransitionNode))]
	class TransitionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.x -= 10f;
			position.width += 10f;
			var listProp = property.FindPropertyRelative("transition");
			listProp.isExpanded = true;
			EditorGUI.PropertyField(position, listProp, null, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var listProp = property.FindPropertyRelative("transition");
			listProp.isExpanded = true;
			return EditorGUI.GetPropertyHeight(listProp, null, true);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using FriendSea.GraphViewSerializer;
using UnityEditor;
using UnityEngine;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("Controls/Random")]
	class RandomNode : IStateMachineNode
	{
		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new Controls.Random()
			{
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};
	}

	class RandomNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(RandomNode);
		public override void Initialize(GraphNode node)
		{
			var image = new UnityEngine.UIElements.Image();
			image.image = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.friendsea.statemachine/Editor/Icons/Random.png");
			node.titleContainer.Insert(0, image);
			node.title = "Random";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMachineGraphSettings.GetColor(typeof(RandomNode));
			node.extensionContainer.Clear();
		}
	}
}

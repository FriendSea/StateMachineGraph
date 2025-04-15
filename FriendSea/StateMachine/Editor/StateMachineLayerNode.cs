using FriendSea.GraphViewSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FriendSea.StateMachine
{
    [DisplayName("Layer")]
    class LayerNode : IStateMachineNode
    {
        public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset)
        {
            throw new System.NotImplementedException();
        }
    }

    class LayerNodeInitializer : GraphNode.IInitializer
    {
        public Type TargetType => typeof(LayerNode);

        public void Initialize(GraphNode node)
        {
            node.title = "Additional Layer";

            var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
            outp.userData = "layer";
            outp.portType = typeof(ISerializableStateReference);
            outp.portColor = Color.white;
            outp.portName = "";
            // outp.style.backgroundColor = new Color(0.1f, 0.1f, 0.5f, 1);
            node.outputContainer.Add(outp);

            /*
            var outp2 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
            outp2.userData = "layer";
            outp2.portType = typeof(ISerializableStateReference);
            outp2.portColor = Color.white;
            outp2.portName = "Fallback";
            outp2.style.backgroundColor = new Color(0.5f, 0.1f, 0.1f, 1);
            node.outputContainer.Add(outp2);
            */
        }
    }
}

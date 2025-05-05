using FriendSea.StateMachine.Conditions;
using FriendSea.StateMachine.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.CullingGroup;
using static UnityEngine.GraphicsBuffer;

namespace FriendSea.StateMachine
{
    [DisplayName("IsNodeActive")]
    partial class NodeActiveTransition : Transition.ICondition
    {
        [SerializeField, NodeId]
        string nodeId;

        public bool IsValid(IContextContainer obj) { 
            foreach(var layer in ((LayeredStateMachine.WrapContext)obj).parent.Layers)
                if (layer.CurrentState.Id.AsSpan().Slice(32).SequenceEqual(nodeId))
                    return true;
            return false;
        }
    }

    [AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    class NodeIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(NodeIdAttribute))]
    class NodeIdDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var ids = new List<string>() { "" };
            var names = new List<string>() { "<null>" };
            var array = property.serializedObject.FindProperty("data.elements");
            for (int i = 0; i < array.arraySize; i++)
            {
                var prop = array.GetArrayElementAtIndex(i);
                if (!prop.FindPropertyRelative("data")?.managedReferenceFullTypename?.EndsWith(".StateNode") ?? true) continue;
                ids.Add(prop.FindPropertyRelative("id.id").stringValue);
                names.Add(prop.FindPropertyRelative("data.name").stringValue);
            }
            var currentIndex = ids.IndexOf(property.stringValue ?? "");
            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, names.ToArray());
            property.stringValue = ids[newIndex];
        }
    }
#endif

#nullable enable

    public class LayeredStateMachine : IDisposable
    {
        internal class WrapContext : ContextContainerBase
        {
            internal LayeredStateMachine parent;
            IContextContainer innterContext;
            public WrapContext(LayeredStateMachine parent, IContextContainer context)
            {
                this.parent = parent;
                innterContext = context;
            }
            public override T Get<T>() =>
                base.Get<T>() ?? innterContext.Get<T>();
        }

        StateMachine<IContextContainer>[] _layers;
        public IEnumerable<StateMachine<IContextContainer>> Layers => _layers;
        public IEnumerable<IState<IContextContainer>> CurrentStates => Layers.Select(l => l.CurrentState);
        public StateMachine<IContextContainer> PrimaryLayer => _layers[0];

        public LayeredStateMachine(IEnumerable<(IStateReference<IContextContainer> entry, IStateReference<IContextContainer> fallback)> layers, IContextContainer context)
        {
            if (layers == null) throw new System.ArgumentNullException();
            if (layers.Count() <= 0) throw new System.ArgumentException();
            _layers = layers.Select(l => new StateMachine<IContextContainer>(l.entry, l.fallback, new WrapContext(this, context))).ToArray();
        }

        public void DoTransition()
        {
            foreach (var layer in _layers)
                layer.DoTransition();
        }

        public void Update(float deltaTime)
        {
            foreach (var layer in _layers)
                layer.Update(deltaTime);
        }

        public void ForceState(IState<IContextContainer> state, int layer = 0) =>
            _layers[layer].ForceState(state);

        public void ForceState(IStateReference<IContextContainer> state, int layer = 0) =>
            _layers[layer].ForceState(state);

        public void Dispose()
        {
            OnDisposiong?.Invoke(this);
            foreach(var l in Layers)
                l.Dispose();
        }

        public event Action<LayeredStateMachine> OnDisposiong;
    }
}

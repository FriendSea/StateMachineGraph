using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace FriendSea.GraphViewSerializer
{
    [SerializableGraphView.TargetData(typeof(GraphViewData.Node), -100)]
    public class GraphNode : Node, SerializableGraphView.ISerializableElement, SerializableGraphView.IPositionableElement
    {
        public interface IInitializer
        {
            System.Type TargetType { get; }
            void Initialize(GraphNode node);
        }

        static Dictionary<System.Type, IInitializer> _inititializers = null;
        static Dictionary<System.Type, IInitializer> Initializers =>
            _inititializers ?? (_inititializers =
                EditorUtils.GetSubClasses(typeof(IInitializer))
                .Select(t => (IInitializer)System.Activator.CreateInstance(t))
                .ToDictionary(i => i.TargetType, i => i)
            );

        public SerializedProperty parentProperty { get; private set; }
        public string id { get; private set; }

        public void Initialize(SerializedProperty property, SerializableGraphView graphView)
        {
            // initialize
            var path = property.propertyPath.Split('.');
            parentProperty = property.serializedObject.FindProperty(path[0]);
            for (int i = 1; i < path.Length - 1; i++)
                this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
            id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;
            title = property.FindPropertyRelative("data").managedReferenceFullTypename.Split('.').Last().Split(" ").Last().Split("/").Last();

            SetPosition(new Rect(property.FindPropertyRelative("position").vector2Value, Vector2.one));

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                var rect = e.newRect;
                rect.x = Mathf.Round(rect.x / 10f) * 10f;
                rect.y = Mathf.Round(rect.y / 10f) * 10f;
                SetPosition(rect);
            });

            if (Initializers.ContainsKey(property.FindPropertyRelative("data").managedReferenceValue.GetType()))
                Initializers[property.FindPropertyRelative("data").managedReferenceValue.GetType()].Initialize(this);
        }

        public void UpdatePosition()
        {
            this.GetProperty().FindPropertyRelative("position").vector2Value = GetPosition().min;
        }

        public Port GetInputPort(string id)
        {
            foreach (Port port in inputContainer.Children())
                if (port.userData as string == id)
                    return port;
            return null;
        }

        public Port GetOutputPort(string id)
        {
            foreach (Port port in outputContainer.Children())
                if (port.userData as string == id)
                    return port;
            return null;
        }

        public void SetupRenamableTitle(string relativeProperyPath)
        {
            var titleLabel = this.Q("title-label") as Label;
            var t = this.GetProperty().FindPropertyRelative(relativeProperyPath).stringValue;
            title = string.IsNullOrEmpty(t) ? "State" : t;

            capabilities |= Capabilities.Renamable;

            titleLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount != 2) return;
                if (e.button != (int)MouseButton.LeftMouse) return;

                RenamePopup.Init(titleLabel.worldBound, title, EndEdit);
            });

            void EndEdit(string newTitle)
            {
                if (string.IsNullOrWhiteSpace(newTitle)) return;

                this.GetProperty().FindPropertyRelative(relativeProperyPath).stringValue = newTitle;
                this.GetProperty().serializedObject.ApplyModifiedProperties();
                this.title = newTitle;
            }
        }
    }
}

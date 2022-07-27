using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace FriendSea
{
	[SerializableGraphView.TargetData(typeof(GraphViewData.Node), -1)]
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

		public void SetupRenamableTitle()
		{
			capabilities |= Capabilities.Renamable;

			var titleLabel = this.Q("title-label") as Label;
			var t = this.GetProperty().FindPropertyRelative("data").FindPropertyRelative("name").stringValue;
			title = string.IsNullOrEmpty(t) ? "State" : t;

			var titleTextField = new TextField { isDelayed = true };
			titleTextField.style.display = DisplayStyle.None;
			titleLabel.parent.Insert(0, titleTextField);

			titleLabel.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					StartEdit();
			});

			titleTextField.RegisterValueChangedCallback(e => EndEdit(e.newValue));

			titleTextField.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					EndEdit(titleTextField.value);
			});

			titleTextField.RegisterCallback<FocusOutEvent>(e => EndEdit(titleTextField.value));

			void StartEdit()
			{
				titleTextField.style.display = DisplayStyle.Flex;
				titleLabel.style.display = DisplayStyle.None;
				titleTextField.focusable = true;

				titleTextField.SetValueWithoutNotify(title);
				titleTextField.Focus();
				titleTextField.SelectAll();
			}

			void EndEdit(string newTitle)
			{
				titleTextField.style.display = DisplayStyle.None;
				titleLabel.style.display = DisplayStyle.Flex;
				titleTextField.focusable = false;

				if (string.IsNullOrEmpty(newTitle)) return;

				this.GetProperty().FindPropertyRelative("data").FindPropertyRelative("name").stringValue = newTitle;
				this.GetProperty().serializedObject.ApplyModifiedProperties();
				this.title = newTitle;
			}
		}
	}
}

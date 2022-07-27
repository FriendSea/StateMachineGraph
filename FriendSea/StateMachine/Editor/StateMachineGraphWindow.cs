using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace FriendSea
{
	public class StateMachineGraphWindow : EditorWindow
	{
		public static void Open(StateMachineImporter importer)
		{
			GetWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow))).LoadAsset(importer);
		}

		[SerializeField]
		StateMachineImporter target;

		void LoadAsset(StateMachineImporter importer)
		{
			target = importer;

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(importer.assetPath) + " (StateMachine)");

			RefleshGraphView();
		}

		void RefleshGraphView()
		{
			if (target == null) return;

			rootVisualElement.Clear();
			var graphView = new SerializableGraphView(this, target.data, new SerializedObject(target).FindProperty("data"), typeof(IStateMachineNode), SetupNode);

			rootVisualElement.Add(graphView);
			rootVisualElement.Add(new Button(target.SaveAndReimport) { text = "Apply" });
		}

		void SetupNode(SerializableGraphView.GraphNode node) {
			// select types

			var prop = node.GetProperty().FindPropertyRelative("data");
			var nodeTypes = new Dictionary<System.Type, Color> {
					{ typeof(IStateMachineNode.State), new Color(1, 0.5f, 0) },
					{ typeof(IStateMachineNode.Transition), Color.green },
				};
			var selfType = prop.managedReferenceValue.GetType();
			var targetType = nodeTypes.Keys.Where(t => t != selfType).FirstOrDefault();

			// fixed nodes

			if (prop.managedReferenceFullTypename == EditorUtils.GetSerializedTypeNameFromType(typeof(StateMachineEntryNode)))
			{
				node.capabilities ^= Capabilities.Deletable;
				node.mainContainer.style.backgroundColor = Color.blue;
				node.title = "Entry";
				var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
				outp.userData = "transitions";
				outp.portType = typeof(IStateMachineNode.State);
				outp.portColor = nodeTypes[typeof(IStateMachineNode.State)];
				outp.portName = "";
				node.outputContainer.Add(outp);
				return;
			}
			if (prop.managedReferenceFullTypename == EditorUtils.GetSerializedTypeNameFromType(typeof(StateMachineFallbackNode)))
			{
				node.capabilities ^= Capabilities.Deletable;
				node.mainContainer.style.backgroundColor = Color.red;
				node.title = "Fallback";
				var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
				outp.userData = "transitions";
				outp.portType = typeof(IStateMachineNode.State);
				outp.portColor = nodeTypes[typeof(IStateMachineNode.State)];
				outp.portName = "";
				node.outputContainer.Add(outp);
				return;
			}

			// add output port

			var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, (targetType == typeof(IStateMachineNode.State)) ? Port.Capacity.Single : Port.Capacity.Multi, typeof(object));
			outport.userData = "transitions";
			outport.portType = targetType;
			outport.portColor = nodeTypes[targetType];
			outport.portName = "";
			node.outputContainer.Add(outport);

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = selfType;
			inport.portColor = nodeTypes[selfType];
			inport.portName = "";
			node.inputContainer.Add(inport);

			// add fields

			node.extensionContainer.Add(new IMGUIContainer(() =>
			{
				node.GetProperty().serializedObject.Update();
				var prop = node.GetProperty().FindPropertyRelative("data");
				int depth = prop.depth;
				prop.NextVisible(true);
				while (true)
				{
					if (prop.depth != depth + 1) continue;
					EditorGUILayout.PropertyField(prop, GUIContent.none, true);
					if (!prop.NextVisible(false)) break;
					if (prop.depth <= depth) break;
				}
				node.GetProperty().serializedObject.ApplyModifiedProperties();
			}));

			// force expanded
			node.titleButtonContainer.Clear();
			node.expanded = true;
			node.RefreshExpandedState();

			if (selfType == typeof(IStateMachineNode.State))
			{
				node.capabilities |= Capabilities.Renamable;
				SetupRenamableTitle(node);
			}

			node.topContainer.Insert(1, node.titleContainer);
			node.mainContainer.style.backgroundColor = nodeTypes[selfType] / 2f;
		}

		void SetupRenamableTitle(SerializableGraphView.GraphNode node)
		{
			var titleLabel = node.Q("title-label") as Label;
			var t = node.GetProperty().FindPropertyRelative("data").FindPropertyRelative("name").stringValue;
			node.title = string.IsNullOrEmpty(t) ? "State" : t;

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

				titleTextField.SetValueWithoutNotify(node.title);
				titleTextField.Focus();
				titleTextField.SelectAll();
			}

			void EndEdit(string newTitle)
			{
				titleTextField.style.display = DisplayStyle.None;
				titleLabel.style.display = DisplayStyle.Flex;
				titleTextField.focusable = false;

				if (string.IsNullOrEmpty(newTitle)) return;

				node.GetProperty().FindPropertyRelative("data").FindPropertyRelative("name").stringValue = newTitle;
				node.GetProperty().serializedObject.ApplyModifiedProperties();
				node.title = newTitle;
			}
		}

		private void OnEnable()
		{
			Undo.undoRedoPerformed += RefleshGraphView;
			RefleshGraphView();
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= RefleshGraphView;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace FriendSea
{
	public static class SerializableElementExtensions
	{
		public static int GetCurrentIndex(this SerializableGraphView.ISerializableElement element)
		{
			for (int i = 0; i < element.parentProperty.arraySize; i++)
			{
				if (element.parentProperty.GetArrayElementAtIndex(i).FindPropertyRelative("id").FindPropertyRelative("id").stringValue == element.id)
					return i;
			}
			return -1;
		}

		public static SerializedProperty GetProperty(this SerializableGraphView.ISerializableElement element) => element.parentProperty.GetArrayElementAtIndex(element.GetCurrentIndex());
	}

	public class SerializableGraphView : GraphView
	{
		public interface ISerializableElement
		{
			string id { get; }
			SerializedProperty parentProperty { get; }
		}

		SerializedProperty dataProperty;
		System.Action<GraphNode> initializeNode;

		public class GraphNode : Node, ISerializableElement
		{
			public SerializedProperty parentProperty { get; private set; }
			public string id { get; private set; }

			public GraphNode(SerializedProperty property)
			{
				// initialize
				var path = property.propertyPath.Split('.');
				parentProperty = property.serializedObject.FindProperty(path[0]);
				for (int i = 1; i < path.Length - 1; i++)
					this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
				id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;
				title = property.FindPropertyRelative("data").managedReferenceFullTypename.Split('.').Last().Split(" ").Last().Split("/").Last();
			}

			public void UpdatePosition()
			{
				this.GetProperty().FindPropertyRelative("position").vector2Value = GetPosition().min;
			}

			public Port GetInputPort(string id)
			{
				foreach(Port port in inputContainer.Children())
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
		}

		public class GraphGroup : Group, ISerializableElement
		{
			public SerializedProperty parentProperty { get; private set; }
			public string id { get;private set; }

			// コールバック内でアセットを更新するかのフラグになる
			bool initialized;

			public GraphGroup(SerializedProperty property, SerializableGraphView graphView)
			{
				var path = property.propertyPath.Split('.');
				parentProperty = property.serializedObject.FindProperty(path[0]);
				for (int i = 1; i < path.Length - 1; i++)
					this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
				id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;
				title = property.FindPropertyRelative("name").stringValue;

				var nodesProp = property.FindPropertyRelative("nodes");
				for(int i = 0; i < nodesProp.arraySize; i++)
					AddElement(graphView.GetNode(nodesProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue));

				var text = headerContainer.Children().First().Children().FirstOrDefault(element => element is TextField) as TextField;
				text.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
				text.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });

				capabilities |= Capabilities.Renamable;
				initialized = true;
			}

			public void UpdatePositions()
			{
				var elements = new HashSet<GraphElement>();
				CollectElements(elements, element => element is GraphNode);
				foreach(var child in elements)
					(child as GraphNode)?.UpdatePosition();
			}

			protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
			{
				if (!initialized) return;
				parentProperty.serializedObject.Update();
				var array = this.GetProperty().FindPropertyRelative("nodes");
				foreach(var element in elements)
				{
					if (!(element is GraphNode)) continue;
					array.InsertArrayElementAtIndex(0);
					array.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = (element as GraphNode).id;
				}
				parentProperty.serializedObject.ApplyModifiedProperties();
			}

			protected override void OnGroupRenamed(string oldName, string newName)
			{
				parentProperty.serializedObject.Update();
				this.GetProperty().FindPropertyRelative("name").stringValue = string.IsNullOrEmpty(newName) ? "Group" : newName;
				parentProperty.serializedObject.ApplyModifiedProperties();
			}
		}

		public SerializableGraphView(EditorWindow editorWindow, GraphViewData data, SerializedProperty dataProperty, System.Type searchDataType, System.Action<GraphNode> onInitializeNode)
		{
			// initialize view

			this.dataProperty = dataProperty;
			this.initializeNode = onInitializeNode;

			this.StretchToParentSize();
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			Insert(0, new GridBackground());

			UpdateViewTransform(dataProperty.FindPropertyRelative("viewPosition").vector3Value, dataProperty.FindPropertyRelative("viewScale").vector3Value);

			var menuWindowProvider = ScriptableObject.CreateInstance<SerializableGraphViewSearchMenuProvider>();
			menuWindowProvider.Initialize(this, editorWindow, searchDataType);
			nodeCreationRequest += context =>
				SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);

			// load nodes

			var elementsProp = dataProperty.FindPropertyRelative("elements");
			for (int i = 0; i < elementsProp.arraySize; i++)
			{
				var prop = elementsProp.GetArrayElementAtIndex(i);
				if (!prop.managedReferenceFullTypename.Contains(nameof(GraphViewData.Node))) continue;
				var node = new GraphNode(prop);
				initializeNode?.Invoke(node);
				node.SetPosition(new Rect(prop.FindPropertyRelative("position").vector2Value, Vector2.one));
				AddElement(node);
			}

			// load edges

			for(int i = 0; i < elementsProp.arraySize; i++)
			{
				var prop = elementsProp.GetArrayElementAtIndex(i);
				if (!prop.managedReferenceFullTypename.Contains(nameof(GraphViewData.Edge))) continue;
				var edge = new Edge();
				edge.output = GetNode(prop.FindPropertyRelative("outputNode").FindPropertyRelative("id").stringValue).GetOutputPort(prop.FindPropertyRelative("outputPort").stringValue);
				edge.input = GetNode(prop.FindPropertyRelative("inputNode").FindPropertyRelative("id").stringValue).GetInputPort(prop.FindPropertyRelative("inputPort").stringValue);
				edge.output.Connect(edge);
				edge.input.Connect(edge);
				AddElement(edge);
			}

			// load groups

			for (int i = 0; i < elementsProp.arraySize; i++)
			{
				var prop = elementsProp.GetArrayElementAtIndex(i);
				if (!prop.managedReferenceFullTypename.Contains(nameof(GraphViewData.Group))) continue;
				var group = new GraphGroup(prop, this);
				AddElement(group);
			}

			// register edit event

			graphViewChanged += change => {
				// move
				if (change.movedElements != null)
					if(change.movedElements.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach(var element in change.movedElements)
						{
							if(element is GraphNode)
								(element as GraphNode).UpdatePosition();
							if (element is GraphGroup)
								(element as GraphGroup).UpdatePositions();
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				// remove
				if (change.elementsToRemove != null)
					if(change.elementsToRemove.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach (var element in change.elementsToRemove)
						{
							if (element is GraphNode)
							{
								elementsProp.DeleteArrayElementAtIndex((element as GraphNode).GetCurrentIndex());
								for (int i = 0; i < elementsProp.arraySize; i++)
								{
									var groupProp = elementsProp.GetArrayElementAtIndex(i);
									if (!groupProp.managedReferenceFullTypename.Contains(nameof(GraphViewData.Group))) continue;
									var groupNodesProp = groupProp.FindPropertyRelative("nodes");
									for(int j = 0; j < groupNodesProp.arraySize; j++)
									{
										var prop = groupNodesProp.GetArrayElementAtIndex(j).FindPropertyRelative("id");
										if (prop.stringValue != (element as GraphNode).id) continue;
										groupNodesProp.DeleteArrayElementAtIndex(i);
										break;
									}
								}
							}
							if(element is Edge)
							{
								for(int i = 0; i < elementsProp.arraySize; i++)
								{
									var prop = elementsProp.GetArrayElementAtIndex(i);
									if (!prop.managedReferenceFullTypename.Contains(nameof(GraphViewData.Edge))) continue;
									if (((element as Edge).output.node as GraphNode).id != prop.FindPropertyRelative("outputNode").FindPropertyRelative("id").stringValue) continue;
									if ((element as Edge).output.userData as string != prop.FindPropertyRelative("outputPort").stringValue) continue;
									if (((element as Edge).input.node as GraphNode).id != prop.FindPropertyRelative("inputNode").FindPropertyRelative("id").stringValue) continue;
									if ((element as Edge).input.userData as string != prop.FindPropertyRelative("inputPort").stringValue) continue;
									elementsProp.DeleteArrayElementAtIndex(i);
									break;
								}
							}
							if(element is GraphGroup)
							{
								elementsProp.DeleteArrayElementAtIndex((element as GraphGroup).GetCurrentIndex());
							}
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				// add edge
				if (change.edgesToCreate != null)
					if (change.edgesToCreate.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach(var edge in change.edgesToCreate)
						{
							/* TODO : avoid dupricate edge.
							if(edgesProp.ArrayAsEnumerable().Any(prop => {
								return
									(prop.FindPropertyRelative("outputNode").stringValue == (edge.output.node as GraphNode).id) &&
									(prop.FindPropertyRelative("outputPort").stringValue == (edge.output as Port).userData as string) &&
									(prop.FindPropertyRelative("inputNode").stringValue == (edge.input.node as GraphNode).id) &&
									(prop.FindPropertyRelative("inputPort").stringValue == (edge.input as Port).userData as string);
							}))
							{
								EditorApplication.delayCall += () => edge.parent.Remove(edge);
								return new GraphViewChange();
							}
							*/
							elementsProp.arraySize++;
							var prop = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
							prop.managedReferenceValue = new GraphViewData.Edge() {
								outputNode = new GraphViewData.Id((edge.output.node as GraphNode).id),
								outputPort = (edge.output as Port).userData as string,
								inputNode = new GraphViewData.Id((edge.input.node as GraphNode).id),
								inputPort = (edge.input as Port).userData as string,
							};
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				return change;
			};
			viewTransformChanged += graph => {
				dataProperty.serializedObject.Update();
				dataProperty.FindPropertyRelative("viewPosition").vector3Value = viewTransform.position;
				dataProperty.FindPropertyRelative("viewScale").vector3Value = viewTransform.scale;
				dataProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			};

			/* TODO : copy paste
			serializeGraphElements = elements => {
				var obj = new GraphViewData();
				var nodes = new List<GraphViewData.Node>();
				foreach(var element in elements)
				{
					if(element is GraphNode)
					{
						nodes.Add(data.elements[(element as GraphNode).currentIndex]);
					}
				}
				obj.elements = nodes.ToArray();
				var result = JsonUtility.ToJson(obj);
				Debug.Log(result);
				return result;
			};
			canPasteSerializedData = str => !string.IsNullOrEmpty(str);
			unserializeAndPaste = (op, str) => {
				var obj = JsonUtility.FromJson<GraphViewData>(str);
				foreach(var node in obj.elements)
				{
					AddNode(node.data, node.position + Vector2.one * 100f);
				}
			};
			*/
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			if (selection.Count() > 0)
			{
				if (selection.Where(item => item is GraphNode).Count() > 0)
					evt.menu.AppendAction("Group Selection", (action) =>
					{
						dataProperty.serializedObject.Update();
						var array = dataProperty.FindPropertyRelative("elements");
						array.arraySize++;
						var prop = array.GetArrayElementAtIndex(array.arraySize - 1);
						prop.managedReferenceValue = new GraphViewData.Group() {
							id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
							name = "New Group",
						};
						dataProperty.serializedObject.ApplyModifiedProperties();

						var group = new GraphGroup(prop, this);
						AddElement(group);
						group.AddElements(selection.Where(selectable => selectable is GraphNode).Select(selectable => selectable as GraphNode));
					});
				if (selection.Where(item => item is GraphGroup).Count() > 0)
					evt.menu.AppendAction("Ungroup", action =>
					{
						foreach (GraphGroup group in selection.Where(selected => selected is GraphGroup))
						{
							var elements = new HashSet<GraphElement>();
							group.CollectElements(elements, element => element is GraphNode);
							group.RemoveElements(elements.Where(element => element is GraphNode));
							group.GetProperty().serializedObject.Update();
							group.GetProperty().FindPropertyRelative("nodes").ClearArray();
							group.GetProperty().serializedObject.ApplyModifiedProperties();
						}
					});
			}
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();
			compatiblePorts.AddRange(ports.ToList().Where(port =>
			{
				if (startPort.node == port.node)
					return false;
				if (port.direction == startPort.direction)
					return false;
				if (port.portType != startPort.portType)
					return false;
				return true;
			}));

			return compatiblePorts;
		}

		public void AddNode(object data, Vector2 position)
		{
			dataProperty.serializedObject.Update();

			var array = dataProperty.FindPropertyRelative("elements");
			array.arraySize++;
			var prop = array.GetArrayElementAtIndex(array.arraySize - 1);
			prop.managedReferenceValue = new GraphViewData.Node() {
				id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
				position = position,
				data = data,
			};

			dataProperty.serializedObject.ApplyModifiedProperties();

			var node = new GraphNode(prop);
			initializeNode?.Invoke(node);
			node.SetPosition(new Rect(position, Vector2.one));

			AddElement(node);
		}

		public GraphNode GetNode(string id)
		{
			foreach(var node in nodes.ToList())
			{
				var graphNode = node as GraphNode;
				if (graphNode == null) continue;
				if (graphNode.id == id)
					return graphNode;
			}
			return null;
		}
	}
}

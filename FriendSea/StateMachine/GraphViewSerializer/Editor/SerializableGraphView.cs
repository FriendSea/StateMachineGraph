using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace FriendSea
{
	public class SerializableGraphView : GraphView
	{
		SerializedProperty dataProperty;
		System.Action<GraphNode> initializeNode;

		public class GraphNode : Node
		{
			SerializedProperty parentProperty;
			internal string id { get; private set; }

			public SerializedProperty property => parentProperty.GetArrayElementAtIndex(currentIndex);
			internal int currentIndex
			{
				get
				{
					for (int i = 0; i < parentProperty.arraySize; i++)
					{
						if (parentProperty.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue == id)
							return i;
					}
					return -1;
				}
			}

			public GraphNode(SerializedProperty property)
			{
				// initialize
				var path = property.propertyPath.Split('.');
				parentProperty = property.serializedObject.FindProperty(path[0]);
				for (int i = 1; i < path.Length - 1; i++)
					this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
				id = property.FindPropertyRelative("id").stringValue;
				title = property.FindPropertyRelative("data").managedReferenceFullTypename.Split('.').Last().Split(" ").Last().Split("/").Last();
			}

			public void UpdatePosition()
			{
				property.FindPropertyRelative("position").vector2Value = GetPosition().min;
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

		public class GraphGroup : Group
		{
			SerializedProperty parentProperty;
			internal string id { get;private set; }

			internal SerializedProperty property => parentProperty.GetArrayElementAtIndex(currentIndex);
			internal int currentIndex
			{
				get
				{
					for (int i = 0; i < parentProperty.arraySize; i++)
					{
						if (parentProperty.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue == id)
							return i;
					}
					return -1;
				}
			}

			// コールバック内でアセットを更新するかのフラグになる
			bool initialized;

			public GraphGroup(SerializedProperty property, SerializableGraphView graphView)
			{
				var path = property.propertyPath.Split('.');
				parentProperty = property.serializedObject.FindProperty(path[0]);
				for (int i = 1; i < path.Length - 1; i++)
					this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
				id = property.FindPropertyRelative("id").stringValue;
				title = property.FindPropertyRelative("name").stringValue;

				var nodesProp = property.FindPropertyRelative("nodes");
				for(int i = 0; i < nodesProp.arraySize; i++)
					AddElement(graphView.GetNode(nodesProp.GetArrayElementAtIndex(i).stringValue));

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
				var array = property.FindPropertyRelative("nodes");
				foreach(var element in elements)
				{
					if (!(element is GraphNode)) continue;
					array.InsertArrayElementAtIndex(0);
					array.GetArrayElementAtIndex(0).stringValue = (element as GraphNode).id;
				}
				parentProperty.serializedObject.ApplyModifiedProperties();
			}

			protected override void OnGroupRenamed(string oldName, string newName)
			{
				parentProperty.serializedObject.Update();
				property.FindPropertyRelative("name").stringValue = string.IsNullOrEmpty(newName) ? "Group" : newName;
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

			var nodesProp = dataProperty.FindPropertyRelative("nodes");
			for (int i = 0; i < nodesProp.arraySize; i++)
			{
				var prop = nodesProp.GetArrayElementAtIndex(i);
				var node = new GraphNode(prop);
				initializeNode?.Invoke(node);
				node.SetPosition(new Rect(prop.FindPropertyRelative("position").vector2Value, Vector2.one));
				AddElement(node);
			}

			// load edges

			var edgesProp = dataProperty.FindPropertyRelative("edges");
			for(int i = 0; i < edgesProp.arraySize; i++)
			{
				var prop = edgesProp.GetArrayElementAtIndex(i);
				var edge = new Edge();
				edge.output = GetNode(prop.FindPropertyRelative("outputNode").stringValue).GetOutputPort(prop.FindPropertyRelative("outputPort").stringValue);
				edge.input = GetNode(prop.FindPropertyRelative("inputNode").stringValue).GetInputPort(prop.FindPropertyRelative("inputPort").stringValue);
				edge.output.Connect(edge);
				edge.input.Connect(edge);
				AddElement(edge);
			}

			// load groups

			var groupsProp = dataProperty.FindPropertyRelative("groups");
			for (int i = 0; i < groupsProp.arraySize; i++)
			{
				var prop = groupsProp.GetArrayElementAtIndex(i);
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
								nodesProp.DeleteArrayElementAtIndex((element as GraphNode).currentIndex);
								for (int i = 0; i < groupsProp.arraySize; i++)
								{
									var groupNodesProp = groupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("nodes");
									for(int j = 0; j < groupNodesProp.arraySize; j++)
									{
										var prop = groupNodesProp.GetArrayElementAtIndex(j);
										if (prop.stringValue != (element as GraphNode).id) continue;
										groupNodesProp.DeleteArrayElementAtIndex(i);
										break;
									}
								}
							}
							if(element is Edge)
							{
								for(int i = 0; i < edgesProp.arraySize; i++)
								{
									var prop = edgesProp.GetArrayElementAtIndex(i);
									if (((element as Edge).output.node as GraphNode).id != prop.FindPropertyRelative("outputNode").stringValue) continue;
									if ((element as Edge).output.userData as string != prop.FindPropertyRelative("outputPort").stringValue) continue;
									if (((element as Edge).input.node as GraphNode).id != prop.FindPropertyRelative("inputNode").stringValue) continue;
									if ((element as Edge).input.userData as string != prop.FindPropertyRelative("inputPort").stringValue) continue;
									edgesProp.DeleteArrayElementAtIndex(i);
									break;
								}
							}
							if(element is GraphGroup)
							{
								groupsProp.DeleteArrayElementAtIndex((element as GraphGroup).currentIndex);
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
							edgesProp.arraySize++;
							var prop = edgesProp.GetArrayElementAtIndex(edgesProp.arraySize - 1);
							prop.FindPropertyRelative("outputNode").stringValue = (edge.output.node as GraphNode).id;
							prop.FindPropertyRelative("outputPort").stringValue = (edge.output as Port).userData as string;
							prop.FindPropertyRelative("inputNode").stringValue = (edge.input.node as GraphNode).id;
							prop.FindPropertyRelative("inputPort").stringValue = (edge.input as Port).userData as string;
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

			serializeGraphElements = elements => {
				var obj = new GraphViewData();
				var nodes = new List<GraphViewData.Node>();
				foreach(var element in elements)
				{
					if(element is GraphNode)
					{
						nodes.Add(data.nodes[(element as GraphNode).currentIndex]);
					}
				}
				obj.nodes = nodes.ToArray();
				var result = JsonUtility.ToJson(obj);
				Debug.Log(result);
				return result;
			};
			canPasteSerializedData = str => !string.IsNullOrEmpty(str);
			unserializeAndPaste = (op, str) => {
				var obj = JsonUtility.FromJson<GraphViewData>(str);
				foreach(var node in obj.nodes)
				{
					AddNode(node.data, node.position + Vector2.one * 100f);
				}
			};
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
						var array = dataProperty.FindPropertyRelative("groups");
						array.arraySize++;
						var prop = array.GetArrayElementAtIndex(array.arraySize - 1);
						prop.FindPropertyRelative("id").stringValue = System.Guid.NewGuid().ToString();
						prop.FindPropertyRelative("name").stringValue = "New Group";
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
							group.property.serializedObject.Update();
							group.property.FindPropertyRelative("nodes").ClearArray();
							group.property.serializedObject.ApplyModifiedProperties();
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

			var array = dataProperty.FindPropertyRelative("nodes");
			array.arraySize++;
			var prop = array.GetArrayElementAtIndex(array.arraySize - 1);
			prop.FindPropertyRelative("id").stringValue = System.Guid.NewGuid().ToString();
			prop.FindPropertyRelative("position").vector2Value = position;
			prop.FindPropertyRelative("data").managedReferenceValue = data;

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

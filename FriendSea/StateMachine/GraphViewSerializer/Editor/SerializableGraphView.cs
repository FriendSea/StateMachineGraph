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
		public class TargetDataAttribute : System.Attribute
		{
			internal System.Type TargetType { get; private set; }
			internal int LoadOrder { get; private set; }

			public TargetDataAttribute(System.Type type, int loadOrder = 0)
			{
				TargetType = type;
				LoadOrder = loadOrder;
			}
		}

		public interface ISerializableElement
		{
			// 配列の中身が増減するとインデックスは変わるので配列自体のプロパティパスとguidで持つ
			string id { get; }
			SerializedProperty parentProperty { get; }

			void Initialize(SerializedProperty property, SerializableGraphView graphView);
		}

		public interface IPositionableElement
		{
			void UpdatePosition();
		}

		SerializedProperty dataProperty;

		public SerializableGraphView(EditorWindow editorWindow, SerializedProperty dataProperty, System.Type searchDataType)
		{
			// initialize view

			this.dataProperty = dataProperty;

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

			// load elements
			var elementsProp = dataProperty.FindPropertyRelative("elements");
			LoadElements(elementsProp.ArrayAsEnumerable().ToList());

			// register edit event

			graphViewChanged += change =>
			{
				// move
				if (change.movedElements != null)
					if (change.movedElements.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach (var element in change.movedElements.Where(e => e is IPositionableElement))
							(element as IPositionableElement).UpdatePosition();
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				// remove
				if (change.elementsToRemove != null)
					if (change.elementsToRemove.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach (var element in change.elementsToRemove)
						{
							if (element is ISerializableElement)
							{
								elementsProp.DeleteArrayElementAtIndex((element as ISerializableElement).GetCurrentIndex());
							}
							if (element is GraphNode)
							{
								// sould remove id from referent group.
								for (int i = 0; i < elementsProp.arraySize; i++)
								{
									var groupProp = elementsProp.GetArrayElementAtIndex(i);
									if (!groupProp.managedReferenceFullTypename.Contains(nameof(GraphViewData.Group))) continue;
									var groupNodesProp = groupProp.FindPropertyRelative("nodes");
									for (int j = 0; j < groupNodesProp.arraySize; j++)
									{
										var prop = groupNodesProp.GetArrayElementAtIndex(j).FindPropertyRelative("id");
										if (prop.stringValue != (element as GraphNode).id) continue;
										groupNodesProp.DeleteArrayElementAtIndex(j);
										break;
									}
								}
							}
							if (element is Edge)
							{
								for (int i = 0; i < elementsProp.arraySize; i++)
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
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				// add edge
				if (change.edgesToCreate != null)
					if (change.edgesToCreate.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach (var edge in change.edgesToCreate)
						{
							// avoid dupricate edge.
							if (elementsProp.ArrayAsEnumerable().Any(prop =>
							{
								return
									(prop.FindPropertyRelative("outputNode")?.FindPropertyRelative("id")?.stringValue == (edge.output.node as GraphNode).id) &&
									(prop.FindPropertyRelative("outputPort")?.stringValue == (edge.output as Port).userData as string) &&
									(prop.FindPropertyRelative("inputNode")?.FindPropertyRelative("id")?.stringValue == (edge.input.node as GraphNode).id) &&
									(prop.FindPropertyRelative("inputPort")?.stringValue == (edge.input as Port).userData as string);
							}))
							{
								Debug.Log("Create dupricate edge : Canceled.");
								EditorApplication.delayCall += () => edge.parent.Remove(edge);
								return new GraphViewChange();
							}

							elementsProp.arraySize++;
							var prop = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
							prop.managedReferenceValue = new GraphViewData.Edge()
							{
								id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
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
			viewTransformChanged += graph =>
			{
				dataProperty.serializedObject.Update();
				dataProperty.FindPropertyRelative("viewPosition").vector3Value = viewTransform.position;
				dataProperty.FindPropertyRelative("viewScale").vector3Value = viewTransform.scale;
				dataProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			};

			// copy paste
			serializeGraphElements = elements =>
			{
				var obj = new GraphViewData();
				foreach (var element in elements)
				{
					if (element is ISerializableElement)
					{
						obj.elements.Add((element as ISerializableElement).GetProperty().managedReferenceValue as GraphViewData.ElementData);
					}
				}
				var result = JsonUtility.ToJson(obj);
				return result;
			};
			canPasteSerializedData = str => !string.IsNullOrEmpty(str);
			unserializeAndPaste = (op, str) =>
			{
				var obj = JsonUtility.FromJson<GraphViewData>(str);

				foreach (var element in obj.elements.Where(e => e is GraphViewData.PositionableElementData).Select(e => e as GraphViewData.PositionableElementData))
					element.position += Vector2.one * 100f;

				// give new guid, keep references
				// GraphViewData.Id must be reference type.
				var idDict = new Dictionary<string, string>();
				foreach (var idObj in obj.elements.SelectMany(e => e.CollectUsedGuids()))
				{
					if (!idDict.ContainsKey(idObj.id))
						idDict.Add(idObj.id, System.Guid.NewGuid().ToString());
					idObj.id = idDict[idObj.id];
				}

				var newProps = new List<SerializedProperty>();
				dataProperty.serializedObject.Update();
				foreach (var element in obj.elements)
				{
					elementsProp.arraySize++;
					var prop = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
					prop.managedReferenceValue = element;
					newProps.Add(prop);
				}
				dataProperty.serializedObject.ApplyModifiedProperties();
				LoadElements(newProps);
			};
		}

		void LoadElements(List<SerializedProperty> elementProps)
		{
			var elementTypes = EditorUtils.GetSubClasses(typeof(ISerializableElement))
				.Where(t => t.IsDefined(typeof(TargetDataAttribute), true))
				.Select(t => new { att = (t.GetCustomAttributes(typeof(TargetDataAttribute), true).FirstOrDefault() as TargetDataAttribute), elementType = t })
				.OrderBy(pair => pair.att.LoadOrder)
				.ToDictionary(pair => pair.att.TargetType, pair => pair.elementType);

			foreach (var pair in elementTypes)
			{
				foreach(var prop in elementProps) { 
					if (prop.managedReferenceValue.GetType() != pair.Key) continue;
					var node = (ISerializableElement)System.Activator.CreateInstance(pair.Value);
					AddElement(node as GraphElement);
					node.Initialize(prop, this);
				}
			}

			// load edges
			foreach(var prop in elementProps)
			{
				if (!prop.managedReferenceFullTypename.Contains(nameof(GraphViewData.Edge))) continue;
				var edge = new Edge();
				edge.output = GetNode(prop.FindPropertyRelative("outputNode").FindPropertyRelative("id").stringValue).GetOutputPort(prop.FindPropertyRelative("outputPort").stringValue);
				edge.input = GetNode(prop.FindPropertyRelative("inputNode").FindPropertyRelative("id").stringValue).GetInputPort(prop.FindPropertyRelative("inputPort").stringValue);
				edge.output.Connect(edge);
				edge.input.Connect(edge);
				AddElement(edge);
			}
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
						prop.managedReferenceValue = new GraphViewData.Group()
						{
							id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
							name = "New Group",
						};
						dataProperty.serializedObject.ApplyModifiedProperties();

						var group = new GraphGroup();
						group.Initialize(prop, this);
						group.AddElements(selection.Where(selectable => selectable is GraphNode).Select(selectable => selectable as GraphNode));
						AddElement(group);
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
			prop.managedReferenceValue = new GraphViewData.Node()
			{
				id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
				position = position,
				data = data,
			};

			dataProperty.serializedObject.ApplyModifiedProperties();

			var node = new GraphNode();
			AddElement(node);
			node.Initialize(prop, this);
		}

		public GraphNode GetNode(string id)
		{
			foreach (var node in nodes.ToList())
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

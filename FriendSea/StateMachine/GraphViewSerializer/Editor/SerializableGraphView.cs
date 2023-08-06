using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;

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

			var grid = new GridBackground();
			Insert(0, grid);
			grid.StretchToParentSize();

			UpdateViewTransform(dataProperty.FindPropertyRelative("viewPosition").vector3Value, dataProperty.FindPropertyRelative("viewScale").vector3Value);

			var menuWindowProvider = ScriptableObject.CreateInstance<SerializableGraphViewSearchMenuProvider>();
			menuWindowProvider.Initialize(this, editorWindow, searchDataType);
			nodeCreationRequest += context =>
				SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);

			var elementsProp = dataProperty.FindPropertyRelative("elements");

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
						foreach (var element in change.elementsToRemove.Where(e => e is ISerializableElement).Select(e => e as ISerializableElement))
						{
							elementsProp.DeleteArrayElementAtIndex(element.GetCurrentIndex());
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
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
						RefleshView();
					}
				// add edge
				if (change.edgesToCreate != null)
					if (change.edgesToCreate.Count() > 0)
					{
						dataProperty.serializedObject.Update();
						foreach (var edge in change.edgesToCreate)
						{
							// to use custom edge, remove added.
							EditorApplication.delayCall += () => RemoveElement(edge);

							// avoid dupricate edge.
							if (elementsProp.ArrayAsEnumerable().Any(prop =>
							{
								return
									(prop.FindPropertyRelative("outputNode")?.FindPropertyRelative("id")?.stringValue == (edge.output.node as GraphNode).id) &&
									(prop.FindPropertyRelative("outputPort")?.stringValue == edge.output.userData as string) &&
									(prop.FindPropertyRelative("inputNode")?.FindPropertyRelative("id")?.stringValue == (edge.input.node as GraphNode).id) &&
									(prop.FindPropertyRelative("inputPort")?.stringValue == edge.input.userData as string);
							}))
							{
								Debug.Log("Create dupricate edge : Canceled.");
								return new GraphViewChange();
							}


							// add edge
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

							var customEdge = new GraphEdge();
							AddElement(customEdge);
							customEdge.Initialize(prop, this);
						}
						dataProperty.serializedObject.ApplyModifiedProperties();
					}
				return change;
			};

			// copy paste
			int pasteCount = 0;
			serializeGraphElements = elements =>
			{
				pasteCount = 0;

				var serializables = elements.Where(e => e is ISerializableElement).Select(e => e as ISerializableElement).ToList();
				var ids = serializables.Select(e => e.id).ToList();
				var obj = new GraphViewData()
				{
					elements = serializables
					.Select(e => e.GetProperty().managedReferenceValue as GraphViewData.ElementData)
					.Where(data => data.CollectDependentGuids().All(id => ids.Contains(id.id)))
					.ToList()
				};
				var result = EditorJsonUtility.ToJson(obj);
				return result;
			};
			canPasteSerializedData = str => !string.IsNullOrEmpty(str);
			unserializeAndPaste = (op, str) =>
			{
				pasteCount++;
				var obj = new GraphViewData();
				EditorJsonUtility.FromJsonOverwrite(str, obj);

				foreach (var element in obj.elements.Where(e => e is GraphViewData.PositionableElementData).Select(e => e as GraphViewData.PositionableElementData))
					element.position += Vector2.one * pasteCount * 100f;

				// give new guid, keep references
				// GraphViewData.Id must be reference type.
				var idDict = new Dictionary<string, string>();
				foreach (var idObj in obj.elements.SelectMany(e => e.CollectDependentGuids()).Concat(obj.elements.Select(e => e.id)))
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

			// register undo
			Undo.undoRedoPerformed += RefleshView;
			RegisterCallback<DetachFromPanelEvent>(e => Undo.undoRedoPerformed -= RefleshView);

			// load elements
			LoadElements(elementsProp.ArrayAsEnumerable().ToList());
		}

		public void UpdateViewTransform()
		{
			dataProperty.serializedObject.Update();
			dataProperty.FindPropertyRelative("viewPosition").vector3Value = viewTransform.position;
			dataProperty.FindPropertyRelative("viewScale").vector3Value = viewTransform.scale;
			dataProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		public void UpdateActiveNode(System.Func<GraphNode, bool> func)
		{
			foreach(GraphNode node in nodes)
				node.style.backgroundColor = new StyleColor(func(node) ? Color.white : Color.clear);
		}

		public void RefleshView()
		{
			foreach (var element in graphElements.Where(e => e is ISerializableElement))
				RemoveElement(element);
			dataProperty.serializedObject.Update();
			var elementsProp = dataProperty.FindPropertyRelative("elements");
			LoadElements(elementsProp.ArrayAsEnumerable().ToList());
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
				foreach (var prop in elementProps.Where(p => p.managedReferenceValue.GetType() == pair.Key))
				{
					var node = (ISerializableElement)System.Activator.CreateInstance(pair.Value);
					AddElement(node as GraphElement);
					node.Initialize(prop, this);
				}
			}
			this.Bind(dataProperty.serializedObject);
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			evt.menu.AppendAction("Add Note", action => {
				dataProperty.serializedObject.Update();
				var array = dataProperty.FindPropertyRelative("elements");
				array.arraySize++;
				var prop = array.GetArrayElementAtIndex(array.arraySize - 1);
				prop.managedReferenceValue = new GraphViewData.StickyNote()
				{
					id = new GraphViewData.Id(System.Guid.NewGuid().ToString()),
					content = "New Note",
				};
				dataProperty.serializedObject.ApplyModifiedProperties();

				var note = new GraphNote();
				note.Initialize(prop, this);
				AddElement(note);
			});
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
			this.Bind(dataProperty.serializedObject);
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

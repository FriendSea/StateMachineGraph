using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;

namespace FriendSea
{
	public class StateMachineGraphWindow : EditorWindow
	{
        public static void Open(StateMachineImporter importer)
        {
            GetWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow))).LoadAsset(importer);
        }

        StateMachineImporter target;
        void LoadAsset(StateMachineImporter importer)
        {
            target = importer;

            titleContent = new GUIContent(Path.GetFileNameWithoutExtension(importer.assetPath) + " (StateMachine)");

            RefleshGraphView();
        }

        void RefleshGraphView()
		{
            rootVisualElement.Clear();

            var graphView = new SerializableGraphView(this, Editor.CreateEditor(target).serializedObject.FindProperty("data"), typeof(StateMachineStateBase), node => {

				// add output ports

				var prop = node.property.FindPropertyRelative("data");
				int depth = prop.depth;
				while (prop.NextVisible(true))
				{
					if (prop.depth <= depth) break;
					if (prop.type != "PPtr<$StateMachineNodeAsset>") continue;
					var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
					outport.portName = prop.displayName;
					outport.userData = prop.name;
					node.outputContainer.Add(outport);
				}

				// fixed nodes

				prop = node.property.FindPropertyRelative("data");
				if (prop.managedReferenceFullTypename == EditorUtils.GetSerializedTypeNameFromType(typeof(StateMachineEntryNode)))
				{
					node.capabilities ^= Capabilities.Deletable;
					node.mainContainer.style.backgroundColor = Color.blue;
					node.title = "Entry";
					return;
				}
				if (prop.managedReferenceFullTypename == EditorUtils.GetSerializedTypeNameFromType(typeof(StateMachineFallbackNode)))
				{
					node.capabilities ^= Capabilities.Deletable;
					node.mainContainer.style.backgroundColor = Color.red;
					node.title = "Fallback";
					return;
				}

				// add input port

				var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
				inport.portName = "Enter";
				inport.userData = "enter";
				node.inputContainer.Add(inport);

				// add fields

				node.extensionContainer.Add(new IMGUIContainer(() => {
					node.property.serializedObject.Update();
					var prop = node.property.FindPropertyRelative("data");
					int depth = prop.depth;
					prop.NextVisible(true);
					while (true)
					{
						if (prop.type != "PPtr<$StateMachineNodeAsset>")
							EditorGUILayout.PropertyField(prop, true);
						if (!prop.NextVisible(false)) break;
						if (prop.depth <= depth) break;
					}
					node.property.serializedObject.ApplyModifiedProperties();
				}));

				// force expanded
				node.titleButtonContainer.Clear();
				node.expanded = true;
				node.RefreshExpandedState();
			});

            rootVisualElement.Add(graphView);
            rootVisualElement.Add(new Button(target.SaveAndReimport) { text = "Apply" });
        }

        public void Awake()
        {
            Undo.undoRedoPerformed += RefleshGraphView;
        }

        public void OnDestroy()
        {
            Undo.undoRedoPerformed -= RefleshGraphView;
		}
    }
}

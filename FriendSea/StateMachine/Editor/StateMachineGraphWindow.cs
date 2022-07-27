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
			var graphView = new SerializableGraphView(this, new SerializedObject(target).FindProperty("data"), typeof(IStateMachineNode));

			rootVisualElement.Add(graphView);
			rootVisualElement.Add(new Button(target.SaveAndReimport) { text = "Apply" });
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

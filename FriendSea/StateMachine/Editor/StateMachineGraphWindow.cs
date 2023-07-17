using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace FriendSea.StateMachine
{
	public class StateMachineGraphWindow : EditorWindow
	{
		public static void Open(string assetPath)
		{
			var window = Resources.FindObjectsOfTypeAll<StateMachineGraphWindow>().FirstOrDefault(w => w.path == assetPath) ?? CreateWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow)), typeof(StateMachineGraphWindow));
			window.LoadAsset(assetPath);
		}

		[SerializeField]
		GraphViewData data;
		[SerializeField]
		string path;

		void LoadAsset(string assetPath)
		{
			Focus();
			path = assetPath;
			data = new GraphViewData();
			EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), data);

			RefleshGraphView();
		}

		void RefleshGraphView()
		{
			if (data == null) return;

			rootVisualElement.Clear();

			var graphView = new SerializableGraphView(this, new SerializedObject(this).FindProperty("data"), typeof(IStateMachineNode));
			rootVisualElement.Add(graphView);

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(path) + " (StateMachine)");

			var saveButton = new Button();
			saveButton.text = "Saved";
			saveButton.style.backgroundColor = new StyleColor(Color.black);
			saveButton.clicked += () =>
			{
				graphView.UpdateViewTransform();

				File.WriteAllText(path, EditorJsonUtility.ToJson(data, true));
				AssetDatabase.Refresh();
				EditorUtility.ClearDirty(this);
				saveButton.text = "Saved";
				saveButton.style.backgroundColor = new StyleColor(Color.black);

				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(path) + " (StateMachine)");
			};
			onDirty = () => {
				saveButton.text = "Save";
				saveButton.style.backgroundColor = new StyleColor(Color.gray);

				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(path) + "* (StateMachine)");
			};
			rootVisualElement.Add(saveButton);
		}

		private void OnEnable()
		{
			RefleshGraphView();
		}

		event System.Action onDirty;
		private void OnValidate()
		{
			EditorApplication.delayCall += () => {
				if (!EditorUtility.IsDirty(this)) return;
				onDirty?.Invoke();
			};
		}
	}
}

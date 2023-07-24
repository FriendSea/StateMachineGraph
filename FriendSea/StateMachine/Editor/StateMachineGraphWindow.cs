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
			var window = Resources.FindObjectsOfTypeAll<StateMachineGraphWindow>().FirstOrDefault(w => AssetDatabase.GUIDToAssetPath(w.guid) == assetPath) ?? CreateWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow)), typeof(StateMachineGraphWindow));
			window.LoadAsset(assetPath);
		}

		[SerializeField]
		GraphViewData data;
		[SerializeField]
		string guid;

		SerializableGraphView graphView;

		void LoadAsset(string assetPath)
		{
			Focus();
			guid = AssetDatabase.AssetPathToGUID(assetPath);
			data = new GraphViewData();
			EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), data);

			RefleshGraphView();
		}

		void RefleshGraphView()
		{
			if (data == null) return;

			rootVisualElement.Clear();

			graphView = new SerializableGraphView(this, new SerializedObject(this).FindProperty("data"), typeof(IStateMachineNode));
			rootVisualElement.Add(graphView);

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");

			var saveButton = new Button();
			saveButton.text = "Saved";
			saveButton.style.backgroundColor = new StyleColor(Color.black);
			saveButton.clicked += () =>
			{
				graphView.UpdateViewTransform();

				File.WriteAllText(AssetDatabase.GUIDToAssetPath(guid), EditorJsonUtility.ToJson(data, true));
				AssetDatabase.Refresh();
				EditorUtility.ClearDirty(this);
				saveButton.text = "Saved";
				saveButton.style.backgroundColor = new StyleColor(Color.black);

				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");
			};
			onDirty = () => {
				saveButton.text = "Save";
				saveButton.style.backgroundColor = new StyleColor(Color.gray);

				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + "* (StateMachine)");
			};
			rootVisualElement.Add(saveButton);
		}

		private void OnEnable()
		{
			RefleshGraphView();
			StateMachine<IContextContainer>.OnStateChanged += OnStateChanged;
		}

		private void OnDisable()
		{
			StateMachine<IContextContainer>.OnStateChanged -= OnStateChanged;
		}

		private void OnStateChanged(string id, IContextContainer target)
		{
			graphView.UpdateActiveNode(node => {
				return (AssetDatabase.AssetPathToGUID(AssetDatabase.GUIDToAssetPath(guid)) + node.id) == id;
			});
		}

		event System.Action onDirty;
		private void OnValidate()
		{
			EditorApplication.delayCall += () =>
			{
				if (!EditorUtility.IsDirty(this)) return;
				onDirty?.Invoke();
			};
		}
	}
}

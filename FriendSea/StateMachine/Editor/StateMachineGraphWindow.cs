using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;

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
		StateMachine<IContextContainer> _selected = null;
		StateMachine<IContextContainer> Selected
		{
			get => _selected;
			set
			{
				if (_selected != null)
					_selected.OnStateChanged -= OnStateChanged;
				_selected = value;
				if (_selected != null)
					_selected.OnStateChanged += OnStateChanged;
				OnStateChanged(_selected?.CurrentState);
			}
		}

		private void OnStateChanged(IState<IContextContainer> obj) =>
			graphView.UpdateActiveNode(node => AssetDatabase.AssetPathToGUID(AssetDatabase.GUIDToAssetPath(guid)) + node.id == obj?.Id);
		private void PlayModeStateChanged(PlayModeStateChange _) =>
			rootVisualElement.Q<ListView>().visible = EditorApplication.isPlaying;

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

			var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
			path = Path.ChangeExtension(path, "uxml");
			AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree(rootVisualElement);

			graphView = new SerializableGraphView(this, new SerializedObject(this).FindProperty("data"), typeof(IStateMachineNode));
			rootVisualElement.Q("GraphArea").Add(graphView);

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");

			var saveButton = rootVisualElement.Q("SaveButton") as Button;
			saveButton.SetEnabled(EditorUtility.IsDirty(this));
			saveButton.clicked += () =>
			{
				graphView.UpdateViewTransform();

				File.WriteAllText(AssetDatabase.GUIDToAssetPath(guid), EditorJsonUtility.ToJson(data, true));
				AssetDatabase.Refresh();
				EditorUtility.ClearDirty(this);
				saveButton.SetEnabled(false);
				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");
			};
			onDirty = () =>
			{
				saveButton.SetEnabled(true);
				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + "* (StateMachine)");
			};

			var listView = rootVisualElement.Q<ListView>();
			listView.visible = EditorApplication.isPlaying;
			listView.makeItem = () => new Label();
			listView.bindItem = (label, index) => (label as Label).text = (listView.itemsSource[index] as GameobjectStateMachine).gameObject.name;
			listView.itemsSource = FindObjectsByType<GameobjectStateMachine>(FindObjectsSortMode.InstanceID);
			listView.onSelectionChange += objects => 
				Selected = ((GameobjectStateMachine)objects.FirstOrDefault())?.StateMachine;
			StateMachine<IContextContainer>.OnInstanceCreated += instance =>
			{
				var items = FindObjectsByType<GameobjectStateMachine>(FindObjectsSortMode.InstanceID);
				listView.itemsSource = items;
				items.First(item => item.StateMachine == null).OnDestroyCalled += StateMachineGraphWindow_OnDestroyCalled;

				if (items.Length != 1) return;
				listView.selectedIndex = 0;
				Selected = instance;
			};
			void StateMachineGraphWindow_OnDestroyCalled(GameobjectStateMachine instance)
			{
				instance.OnDestroyCalled -= StateMachineGraphWindow_OnDestroyCalled;
				listView.itemsSource = FindObjectsByType<GameobjectStateMachine>(FindObjectsSortMode.InstanceID);
			}
		}

		private void OnEnable()
		{
			RefleshGraphView();
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
		}

		private void OnDisable() =>
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;

		event System.Action onDirty;
		private void OnValidate() =>
			EditorApplication.delayCall += () =>
			{
				if (!EditorUtility.IsDirty(this)) return;
				onDirty?.Invoke();
			};
	}
}

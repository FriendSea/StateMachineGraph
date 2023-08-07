using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor.UIElements;

namespace FriendSea.StateMachine
{
	public class StateMachineGraphWindow : EditorWindow
	{
		const string saveOnPlaySettingKey = "friendseastatemachinesaveonplay";

		public static void Open(string assetPath)
		{
			var window = Resources.FindObjectsOfTypeAll<StateMachineGraphWindow>().FirstOrDefault(w => AssetDatabase.GUIDToAssetPath(w.guid) == assetPath);
			if (window == null)
			{
				window = CreateWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow)), typeof(StateMachineGraphWindow));
				window.LoadAsset(assetPath);
			}
			window.Focus();
		}

		[SerializeField]
		GraphViewData data;
		[SerializeField]
		string guid;

		bool SaveOnPlay
		{
			get => !string.IsNullOrEmpty(EditorUserSettings.GetConfigValue(saveOnPlaySettingKey));
			set => EditorUserSettings.SetConfigValue(saveOnPlaySettingKey, value ? "yes!" : null);
		}

		SerializableGraphView graphView;
		StateMachine<IContextContainer> _selected = null;
		Button saveButton;
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

		private void PlayModeStateChanged(PlayModeStateChange change)
		{
			rootVisualElement.Q<ListView>().visible = EditorApplication.isPlaying;
			if (change == PlayModeStateChange.ExitingEditMode && SaveOnPlay)
				SaveAsset();
		}

		void LoadAsset(string assetPath)
		{
			guid = AssetDatabase.AssetPathToGUID(assetPath);
			data = new GraphViewData();
			EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), data);
			EditorUtility.ClearDirty(this);
			RefleshGraphView();
		}

		void RefleshGraphView()
		{
			if (data == null) return;

			rootVisualElement.Clear();

			var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
			path = Path.ChangeExtension(path, "uxml");
			AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree(rootVisualElement);

			var so = new SerializedObject(this);

			graphView = new SerializableGraphView(this, so.FindProperty("data"), typeof(IStateMachineNode));
			rootVisualElement.Q("GraphArea").Add(graphView);
			path = Path.ChangeExtension(path, "uss");
			graphView.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");

			saveButton = rootVisualElement.Q<Button>("SaveButton");
			saveButton.SetEnabled(EditorUtility.IsDirty(this));
			saveButton.clicked += SaveAsset;
			onDirty = () =>
			{
				saveButton.SetEnabled(true);
				titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + "* (StateMachine)");
			};

			rootVisualElement.Q<Button>("FitButton").clicked += graphView.FitToContainer;

			var saveOnPlay = rootVisualElement.Q<Toggle>("SaveOnPlay");
			saveOnPlay.value = SaveOnPlay;
			saveOnPlay.RegisterValueChangedCallback(e => SaveOnPlay = e.newValue);

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

		void SaveAsset()
		{
			graphView.UpdateViewTransform();

			File.WriteAllText(AssetDatabase.GUIDToAssetPath(guid), EditorJsonUtility.ToJson(data, true));
			AssetDatabase.Refresh();
			EditorUtility.ClearDirty(this);
			saveButton.SetEnabled(false);
			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");
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

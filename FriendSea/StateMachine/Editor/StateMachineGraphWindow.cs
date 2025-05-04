using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using FriendSea.GraphViewSerializer;
using FriendSea.StateMachine.Behaviours;
using System;

namespace FriendSea.StateMachine
{
    class StateMachineGraphWindow : EditorWindow
    {
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
        StateMachineGraphData data;
        [SerializeReference]
        List<GraphViewData.ElementData> lockedElements;
        [SerializeField]
        string guid;

        SerializableGraphView graphView;
        List<StateMachine<IContextContainer>> _selected = null;
        Button saveButton;
        IEnumerable<StateMachine<IContextContainer>> Selected
        {
            get => _selected;
            set
            {
                if (_selected != null)
                    foreach (var item in _selected)
                        item.OnStateChanged -= OnStateChanged;
                _selected = value.ToList();
                foreach (var item in _selected)
                    item.OnStateChanged += OnStateChanged;
                OnStateChanged(default);
            }
        }


        private void OnStateChanged(IState<IContextContainer> _)
        {
            if (_selected == null) return;
            graphView.UpdateActiveNode(node => _selected.Any(l => l.CurrentState.Id.AsSpan().Slice(32).SequenceEqual(node.id)));
        }

        private void PlayModeStateChanged(PlayModeStateChange change)
        {
            rootVisualElement.Q<ListView>().visible = EditorApplication.isPlaying;
            if (change == PlayModeStateChange.ExitingEditMode && StateMachineGraphSettings.SaveOnPlay)
                SaveAsset();
        }

        void LoadAsset(string assetPath)
        {
            guid = AssetDatabase.AssetPathToGUID(assetPath);
            data = StateMachineGraphData.FromJson(assetPath);

            EditorUtility.ClearDirty(this);
            RefleshGraphView();
        }

        void RefleshGraphView()
        {
            if (data == null) return;
            lockedElements = data.GetBaseElements();

            rootVisualElement.Clear();

            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            path = Path.ChangeExtension(path, "uxml");
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree(rootVisualElement);

            var so = new SerializedObject(this);

            graphView = new SerializableGraphView(this, so.FindProperty("data"), typeof(IStateMachineNode), typeof(IStateMachineDropHandler), so.FindProperty("lockedElements"));
            rootVisualElement.Q("GraphArea").Add(graphView);
            path = Path.ChangeExtension(path, "uss");
            graphView.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
            graphView.SetupAdditionalContextualMenu += evt =>
            {
                var node = graphView.selection.FirstOrDefault(item => item is GraphNode);
                if (node == null) return;
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Copy Break Condition for Debug", _ => EditorGUIUtility.systemCopyBuffer = $"Id ==\"{guid}{(node as GraphNode).id}\"");
            };

            titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)");

            saveButton = rootVisualElement.Q<Button>("SaveButton");
            saveButton.SetEnabled(IsDirty);
            saveButton.clicked += SaveAsset;
            onDirty = () =>
            {
                saveButton.SetEnabled(true);
                titleContent = new GUIContent(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + "* (StateMachine)");
            };

            rootVisualElement.Q<Button>("ShowInProject").clicked += () =>
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<StateMachineAsset>(AssetDatabase.GUIDToAssetPath(guid)));

            rootVisualElement.Q<Button>("FitButton").clicked += graphView.FitToContainer;

            var settingButton = rootVisualElement.Q<Button>("SettingsButton");
            settingButton.clicked += () =>
                UnityEditor.PopupWindow.Show(settingButton.worldBound, new StateMachineGraphSettings(graphView.RefleshView));

            var saveOnPlay = rootVisualElement.Q<Toggle>("SaveOnPlay");
            saveOnPlay.value = StateMachineGraphSettings.SaveOnPlay;
            saveOnPlay.RegisterValueChangedCallback(e => StateMachineGraphSettings.SaveOnPlay = e.newValue);

            var listView = rootVisualElement.Q<ListView>();
            listView.visible = EditorApplication.isPlaying;
            listView.makeItem = () => new Label();
            listView.bindItem = (label, index) => (label as Label).text = (listView.itemsSource[index] as GameobjectStateMachine).gameObject.name;
            listView.itemsSource = FindObjectsByType<GameobjectStateMachine>(FindObjectsSortMode.InstanceID);
            listView.selectionChanged += objects =>
                Selected = objects.SelectMany(o => (o as GameobjectStateMachine).StateMachine.Layers);
            GameobjectStateMachine.OnCreated += instance =>
            {
                var items = FindObjectsByType<GameobjectStateMachine>(FindObjectsSortMode.InstanceID);
                listView.itemsSource = items;
                instance.OnDestroyCalled += StateMachineGraphWindow_OnDestroyCalled;

                if (items.Length != 1) return;
                listView.selectedIndex = 0;
                Selected = items.First().StateMachine.Layers;
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
            StateMachineImporter.OnImport += OnImport;
        }

        private void OnImport(string obj)
        {
            if (obj != guid) return;
            LoadAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            StateMachineImporter.OnImport -= OnImport;
        }

        private void OnDestroy()
        {
            if (!IsDirty) return;
            var result = EditorUtility.DisplayDialogComplex(
                "StateMachine not saved",
                $"{Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) + " (StateMachine)"} is not saved.",
                "Save and close",
                "Cancel",
                "Discard change");
            if (result == 0)
                SaveAsset();
            if (result == 1)
            {
                var window = Instantiate(this);
                window.Show();
                window.Focus();
            }
        }

        bool IsDirty => EditorUtility.IsDirty(this);

        event System.Action onDirty;
        private void OnValidate() =>
            EditorApplication.delayCall += () =>
            {
                if (!IsDirty) return;
                onDirty?.Invoke();
            };
    }
}

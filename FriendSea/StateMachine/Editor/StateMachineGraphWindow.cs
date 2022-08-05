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
		public static void Open(string assetPath)
		{
			GetWindow<StateMachineGraphWindow>(ObjectNames.NicifyVariableName(nameof(StateMachineGraphWindow))).LoadAsset(assetPath);
		}

		[SerializeField]
		GraphViewData data;
		[SerializeField]
		string path;

		void LoadAsset(string assetPath)
		{
			path = assetPath;
			data = JsonUtility.FromJson<GraphViewData>(File.ReadAllText(assetPath));

			titleContent = new GUIContent(Path.GetFileNameWithoutExtension(assetPath) + " (StateMachine)");

			RefleshGraphView();
		}

		void RefleshGraphView()
		{
			if (data == null) return;

			rootVisualElement.Clear();
			var graphView = new SerializableGraphView(this, new SerializedObject(this).FindProperty("data"), typeof(IStateMachineNode));

			rootVisualElement.Add(graphView);
			rootVisualElement.Add(new Button(()=> {
				File.WriteAllText(path, JsonUtility.ToJson(data));
				AssetDatabase.Refresh();
			}) { text = "Save" });
		}

		private void OnEnable()
		{
			RefleshGraphView();
		}
	}
}

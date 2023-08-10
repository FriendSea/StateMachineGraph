using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FriendSea.StateMachine
{
	internal class StateMavhineGraphSettings : PopupWindowContent
	{
		const string saveOnPlayKey = "friendseastatemachinesaveonplay";

		public static bool SaveOnPlay
		{
			get => !string.IsNullOrEmpty(EditorUserSettings.GetConfigValue(saveOnPlayKey));
			set => EditorUserSettings.SetConfigValue(saveOnPlayKey, value ? "yes!" : null);
		}

		static Dictionary<System.Type, Color> defaultColors = new Dictionary<System.Type, Color>() {
			{ typeof(StateNode),  new Color(1f, 0.5f, 0f)/2f},
			{ typeof(TransitionNode), Color.green/2f },
			{ typeof(ResidentStateNode), Color.yellow/2f },
			{ typeof(SequenceNode), Color.black },
			{ typeof(StateMachineReferenceNode), Color.blue/2f },
		};
		public static void AddDefaultColor(System.Type type, Color color) =>
			defaultColors.Add(type, color);

		const string NodeColorKey = "friendseastatemachinecolor";
		public static Color GetColor(System.Type type) =>
			ColorUtility.TryParseHtmlString(EditorUserSettings.GetConfigValue($"{NodeColorKey}_{type.Name}"), out var color) ?
					color :
					(defaultColors.ContainsKey(type) ? defaultColors[type] : Color.black);
		public static void SetColor(System.Type type, Color color) =>
			EditorUserSettings.SetConfigValue($"{NodeColorKey}_{type.Name}", "#" + ColorUtility.ToHtmlStringRGBA(color));

		event System.Action onClose;
		public StateMavhineGraphSettings(System.Action onClose) => this.onClose = onClose;

		public override void OnGUI(Rect rect)
		{
			rect.height = EditorGUIUtility.singleLineHeight;
			foreach(var type in EditorUtils.GetSubClasses(typeof(IStateMachineNode)))
			{
				SetColor(type, EditorGUI.ColorField(rect, new GUIContent(type.Name), GetColor(type)));
				rect.y += EditorGUIUtility.singleLineHeight + 2f;
			}

			if(GUI.Button(rect, "Reset Colors"))
				foreach (var type in defaultColors.Keys)
					SetColor(type, defaultColors[type]);
		}

		public override void OnClose() => onClose?.Invoke();
		public override Vector2 GetWindowSize() =>
			new Vector2(300f, (EditorUtils.GetSubClasses(typeof(IStateMachineNode)).Count + 1) * (EditorGUIUtility.singleLineHeight + 2f));
	}
}

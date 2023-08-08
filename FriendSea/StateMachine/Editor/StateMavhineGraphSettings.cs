using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal class StateMavhineGraphSettings : PopupWindowContent
{
	const string saveOnPlayKey = "friendseastatemachinesaveonplay";

	public static bool SaveOnPlay
	{
		get => !string.IsNullOrEmpty(EditorUserSettings.GetConfigValue(saveOnPlayKey));
		set => EditorUserSettings.SetConfigValue(saveOnPlayKey, value ? "yes!" : null);
	}

	const string StateColorKey = "friendseastatemachinestatecolor";
	public static Color StateColor {
		get => ColorUtility.TryParseHtmlString(EditorUserSettings.GetConfigValue(StateColorKey), out var color) ?
					color :
					new Color(1, 0.5f, 0) / 2f;
		set => EditorUserSettings.SetConfigValue(StateColorKey, "#" + ColorUtility.ToHtmlStringRGBA(value));
	}

	const string ConditionColorKey = "friendseastatemachineconditioncolor";
	public static Color ConditionColor
	{
		get => ColorUtility.TryParseHtmlString(EditorUserSettings.GetConfigValue(ConditionColorKey), out var color) ?
				color :
				Color.green / 2f;
		set => EditorUserSettings.SetConfigValue(ConditionColorKey, "#" + ColorUtility.ToHtmlStringRGBA(value));
	}

	event System.Action onClose;
	public StateMavhineGraphSettings(System.Action onClose) => this.onClose = onClose;

	public override void OnGUI(Rect rect)
	{
		var originalLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 100f;
		rect.height = EditorGUIUtility.singleLineHeight;
		
		StateColor = EditorGUI.ColorField(rect, new GUIContent("State Nodes"), StateColor);
		rect.y += EditorGUIUtility.singleLineHeight + 2f;

		ConditionColor = EditorGUI.ColorField(rect, new GUIContent("Condition Nodes"), ConditionColor);
		rect.y += EditorGUIUtility.singleLineHeight + 2f;

		EditorGUIUtility.labelWidth = originalLabelWidth;
	}

	public override void OnClose() => onClose?.Invoke();
}

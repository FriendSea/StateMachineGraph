using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FriendSea.GraphViewSerializer
{
    class RenamePopup : UnityEditor.EditorWindow
    {
        public static void Init(Rect position, string value, Action<string> resultCallback)
        {
            RenamePopup window = CreateInstance<RenamePopup>();
            window.position = GUIUtility.GUIToScreenRect(position);

            var text = new TextField() { isDelayed = true };
            text.value = value;
            text.RegisterValueChangedCallback(e =>
            {
                resultCallback(text.value);
                window.Close();
            });
            window.rootVisualElement.Add(text);

            window.ShowPopup();
            text.Focus();
        }

        void OnLostFocus()
        {
            Close();
        }
    }
}

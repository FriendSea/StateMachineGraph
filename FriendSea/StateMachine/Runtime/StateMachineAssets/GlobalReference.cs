using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FriendSea.StateMachine
{
    [Serializable]
    public class GlobalReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        string globalId;
        [SerializeField]
        string path;

#if UNITY_EDITOR
        internal void FixForBuild()
        {
            GlobalObjectId.TryParse(globalId, out var id);
            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            path = GetPath(((obj as GameObject) ?? (obj as Component).gameObject).transform);

            static string GetPath(Transform transform)
            {
                if(transform.parent == null)
                    return transform.name;
                return $"{GetPath(transform.parent)}/{transform.name}";
            }
        }
#endif

        [NonSerialized]
        T _value;
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    var obj = GameObject.Find(path);
                    if (typeof(T) == typeof(GameObject))
                        _value = obj as T;
                    else
                        _value = obj.GetComponent<T>();
                }
                return _value;
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GlobalReference<>))]
    class GlobalReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative("globalId");
            GlobalObjectId.TryParse(idProp.stringValue, out var id);
            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            var newObj = EditorGUI.ObjectField(position, label, obj, typeof(UnityEngine.Object), true);
            if (newObj != obj)
                idProp.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(newObj).ToString();
        }
    }
#endif

    partial class GlobalReferenceTestBehaviour : IBehaviour
    {
        [SerializeField]
        GlobalReference<GameObject> globalReference;

        public void OnEnter(IContextContainer obj)
        {

        }

        public void OnExit(IContextContainer obj)
        {

        }

        public void OnUpdate(IContextContainer obj)
        {

        }
    }
}

using FriendSea.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameobjectStateMachine))]
public class GameobjectStateMachineEditor : Editor
{
    IEnumerable<IInjectable> GetInjectables()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(serializedObject.FindProperty("asset").objectReferenceValue));
        foreach (var so in assets.Select(a => new SerializedObject(a)))
        {
            var itt = so.GetIterator();
            while (itt.NextVisible(true))
            {
                if (itt.propertyType != SerializedPropertyType.ManagedReference) continue;
                if (itt.managedReferenceValue is not IInjectable) continue;
                yield return itt.managedReferenceValue as IInjectable;
            }
        }
    }

    Type[] reqiredTypes;

    void CollectReqiredTypes() =>
        reqiredTypes = GetInjectables().SelectMany(i => i.GetRequiredTypes()).Distinct().ToArray();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CollectReqiredTypes();
        foreach (var type in reqiredTypes)
        {
            if ((target as GameobjectStateMachine).gameObject.GetComponentInChildren(type) == null)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var icon = EditorGUIUtility.IconContent("console.warnicon");
                        GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));

                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField($"StateMachine needs {type.Name} as context.", EditorStyles.wordWrappedLabel);
                            var types = TypeCache.GetTypesDerivedFrom(type).Prepend(type).Where(t => !t.IsInterface && !t.IsAbstract).ToArray();
                            var selected = EditorGUILayout.Popup(0, types.Select(t => $"Add {t.Name}").Prepend("Fix...").ToArray());
                            if(selected != 0)
                                (target as GameobjectStateMachine).gameObject.AddComponent(types[selected]);
                        }
                    }
                }
            }
        }
    }
}

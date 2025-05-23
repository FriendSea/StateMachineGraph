using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace FriendSea.StateMachine
{
    class JsonContainer
    {
        public References references;
        public class References
        {
            public class RefId
            {
                public int rid;
            }

            public RefId[] RefIds;
        }
    }

    [CustomEditor(typeof(StateMachineImporter))]
    class StateMachineImporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Graph Editor"))
                StateMachineGraphWindow.Open((target as StateMachineImporter).assetPath);
            using (new EditorGUI.DisabledGroupScope(true))
                base.OnInspectorGUI();
        }

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var target = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID))) as StateMachineImporter;
            if (target == null) return false;
            StateMachineGraphWindow.Open(target.assetPath);
            return true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using FriendSea.GraphViewSerializer;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace FriendSea.StateMachine
{
    [System.Serializable]
    class StateMachineGraphData : GraphViewData
    {
        public string baseAssetGuid;

        public static StateMachineGraphData FromJson(string path)
        {
			var data = new StateMachineGraphData();
			var json = File.ReadAllText(path);

			EditorJsonUtility.FromJsonOverwrite(json, data);
			if (data.elements.Any(d => d == null))
			{
				json = SerializesJsonUtils.NullifyMissingReferences(json);
				EditorJsonUtility.FromJsonOverwrite(json, data);
			}

			return data;
		}

		public List<ElementData> GetBaseElements()
		{
			if (string.IsNullOrEmpty(baseAssetGuid)) return null;
			return FromJson(AssetDatabase.GUIDToAssetPath(baseAssetGuid)).elements;
		}
    }
}

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
		[System.Serializable]
		public class IdNamePair
		{
			public int id;
			public string name;
		}

		public List<IdNamePair> variables = new List<IdNamePair>();
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
			return FromJson(BaseAssetPath).elements;
		}

		public void UnpackBaseAsset()
		{
			var baseElements = GetBaseElements();
			if (baseElements != null)
				elements.AddRange(baseElements);
			baseAssetGuid = null;
		}

		public string BaseAssetPath => AssetDatabase.GUIDToAssetPath(baseAssetGuid);
    }
}

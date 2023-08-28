using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class Sequence : ISerializableStateReference
	{
		class Context
		{
			Dictionary<object, int> values = new Dictionary<object, int>();
			public int GetValue(object target)
			{
				if (!values.ContainsKey(target))
					values.Add(target, 0);
				return values[target];
			}
			public int SetValue(object target, int value) => values[target] = value;
		}

		[SerializeReference]
		public ISerializableStateReference[] targets;
		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			// 遷移先がない、nullに遷移
			if (targets.Length <= 0) return (null, true);

			var indexContext = obj.GetOrCreate<Context>();
			var currentIndex = indexContext.GetValue(this);
			indexContext.SetValue(this, (currentIndex + 1) % targets.Length);

			// 現在のインデックスの遷移先
			var result = targets[currentIndex].GetState(obj);
			return result.isValid ?
				(result.state, true) :
				(null, true);
		}
	}
}

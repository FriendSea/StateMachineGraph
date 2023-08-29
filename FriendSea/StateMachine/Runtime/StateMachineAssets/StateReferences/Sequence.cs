using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Controls
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

			// 現在のインデックスの遷移先
			for (; currentIndex < targets.Length; currentIndex++)
			{
				var result = targets[currentIndex].GetState(obj);
				if (!result.isValid) break;
				if (result.state == null) continue;
				indexContext.SetValue(this, currentIndex + 1);
				obj.Get<StateMachine<IContextContainer>>().PushReturnState(this);
				return (result.state, true);
			}
			indexContext.SetValue(this, 0);
			return (null, true);
		}
	}
}

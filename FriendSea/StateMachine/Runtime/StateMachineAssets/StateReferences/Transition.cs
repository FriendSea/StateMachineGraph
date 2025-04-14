using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Controls
{
	[System.Serializable]
	public struct Transition : ISerializableStateReference
	{
		public interface ICondition
		{
			bool IsValid(IContextContainer obj);
		}

		[SerializeReference]
		public ICondition condition;
		[SerializeReference]
		public ISerializableStateReference[] targets;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			// 遷移なし
			if (condition == null || targets == null) return (null, false);
			// 条件にマッチしてない。遷移しない。
			(condition as IInjectable)?.OnSetup(obj);
			if (!condition.IsValid(obj)) return (null, false);
			// マッチしたが遷移先がない、nullに遷移
			if (targets.Length <= 0) return (null, true);

			// 接続先の最初の有効なものに遷移
			foreach (var target in targets)
			{
				var result = target.GetState(obj);
				if (result.isValid) return (result.state, true);
			}
			// 何も有効じゃなかった。遷移しない。
			return (null, false);
		}
	}

	[DisplayName("Hidden/Always")]
	class ImmediateTransition : Transition.ICondition
	{
		public bool IsValid(IContextContainer obj) => true;
	}
}

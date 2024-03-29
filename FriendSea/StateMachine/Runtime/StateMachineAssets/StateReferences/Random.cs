using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Controls
{
	[System.Serializable]
	public class Random : ISerializableStateReference
	{
		[SerializeReference]
		public ISerializableStateReference[] targets;
		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			// 遷移先がない、nullに遷移
			if (targets.Length <= 0) return (null, true);

			// ランダム遷移
			return targets[UnityEngine.Random.Range(0, targets.Length)].GetState(obj);
		}
	}
}

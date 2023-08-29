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
			// �J�ڐ悪�Ȃ��Anull�ɑJ��
			if (targets.Length <= 0) return (null, true);

			// �����_���J��
			return targets[UnityEngine.Random.Range(0, targets.Length)].GetState(obj);
		}
	}
}

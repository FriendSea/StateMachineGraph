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
			// �J�ڂȂ�
			if (condition == null || targets == null) return (null, false);
			// �����Ƀ}�b�`���ĂȂ��B�J�ڂ��Ȃ��B
			(condition as IInjectable)?.OnSetup(obj);
			if (!condition.IsValid(obj)) return (null, false);
			// �}�b�`�������J�ڐ悪�Ȃ��Anull�ɑJ��
			if (targets.Length <= 0) return (null, true);

			// �ڑ���̍ŏ��̗L���Ȃ��̂ɑJ��
			foreach (var target in targets)
			{
				var result = target.GetState(obj);
				if (result.isValid) return (result.state, true);
			}
			// �����L������Ȃ������B�J�ڂ��Ȃ��B
			return (null, false);
		}
	}

	[DisplayName("Hidden/Always")]
	class ImmediateTransition : Transition.ICondition
	{
		public bool IsValid(IContextContainer obj) => true;
	}
}

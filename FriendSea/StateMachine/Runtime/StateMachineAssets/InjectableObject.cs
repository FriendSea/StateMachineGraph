using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class InjectableObjectBase {
		protected virtual void OnSetup(IContextContainer ctx) { }
	}

	public abstract class BehaviourBase : InjectableObjectBase, IBehaviour
	{
		void IBehaviour.OnEnter(IContextContainer obj)
		{
			OnSetup(obj);
			OnEnter(obj);
		}
		void IBehaviour.OnExit(IContextContainer obj)
		{
			OnSetup(obj);
			OnExit(obj);
		}
		void IBehaviour.OnUpdate(IContextContainer obj)
		{
			OnSetup(obj);
			OnUpdate(obj);
		}
		protected abstract void OnEnter(IContextContainer obj);
		protected abstract void OnExit(IContextContainer obj);
		protected abstract void OnUpdate(IContextContainer obj);
	}
	public abstract class ConditionBase : InjectableObjectBase, State.Transition.ICondition
	{
		bool State.Transition.ICondition.IsValid(IContextContainer obj)
		{
			OnSetup(obj);
			return IsValid(obj);
		}
		protected abstract bool IsValid(IContextContainer obj);
	}
}

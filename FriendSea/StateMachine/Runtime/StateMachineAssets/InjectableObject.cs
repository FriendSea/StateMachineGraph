using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class InjectableObjectBase {
		protected virtual void OnSetup(IContextContainer ctx) { }
	}

	public abstract class BehaviourBase : InjectableObjectBase, State.IBehaviour
	{
		void State.IBehaviour.OnEnter(IContextContainer obj, int frameCount)
		{
			OnSetup(obj);
			OnEnter(obj, frameCount);
		}
		void State.IBehaviour.OnExit(IContextContainer obj, int frameCount)
		{
			OnSetup(obj);
			OnExit(obj, frameCount);
		}
		void State.IBehaviour.OnUpdate(IContextContainer obj, int frameCount)
		{
			OnSetup(obj);
			OnUpdate(obj, frameCount);
		}
		protected abstract void OnEnter(IContextContainer obj, int frameCount);
		protected abstract void OnExit(IContextContainer obj, int frameCount);
		protected abstract void OnUpdate(IContextContainer obj, int frameCount);
	}
	public abstract class ConditionBase : InjectableObjectBase, State.Transition.ICondition
	{
		bool State.Transition.ICondition.IsValid(IContextContainer obj, int frameCount)
		{
			OnSetup(obj);
			return IsValid(obj, frameCount);
		}
		protected abstract bool IsValid(IContextContainer obj, int frameCount);
	}
}

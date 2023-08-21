using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class InjectableObjectBase { }

	public abstract class BehaviourBase : InjectableObjectBase, State.IBehaviour
	{
		public virtual void OnSetup(IContextContainer obj, int frameCount) { }
		public abstract void OnEnter(IContextContainer obj, int frameCount);

		public abstract void OnExit(IContextContainer obj, int frameCount);

		public abstract void OnUpdate(IContextContainer obj, int frameCount);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class InjectableObjectBase {
		protected virtual void OnSetup(IContextContainer obj, int frameCount) { }
	}

	public abstract class BehaviourBase : InjectableObjectBase, State.IBehaviour
	{
		void State.IBehaviour.OnEnter(IContextContainer obj, int frameCount)
		{
			OnSetup(obj, frameCount);
			OnEnter(obj, frameCount);
		}

		void State.IBehaviour.OnExit(IContextContainer obj, int frameCount)
		{
			OnSetup(obj, frameCount);
			OnExit(obj, frameCount);
		}

		void State.IBehaviour.OnUpdate(IContextContainer obj, int frameCount)
		{
			OnSetup(obj, frameCount);
			OnUpdate(obj, frameCount);
		}

		public abstract void OnEnter(IContextContainer obj, int frameCount);

		public abstract void OnExit(IContextContainer obj, int frameCount);

		public abstract void OnUpdate(IContextContainer obj, int frameCount);
	}
}

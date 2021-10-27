using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public abstract class StateMachineStateBase : IStateMachineState<GameObject>
	{
		public virtual IStateMachineState<GameObject> NextState(GameObject obj, int frameCount) => this;

		public virtual void OnExit(GameObject obj, int frameCount) { }

		public abstract void OnUpdate(GameObject obj, int frameCount);
	}
}

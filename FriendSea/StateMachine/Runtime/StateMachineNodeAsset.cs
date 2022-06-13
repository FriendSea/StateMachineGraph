using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea {
	public class StateMachineNodeAsset : ScriptableObject, IStateMachineState<CachedComponents>
	{
		[SerializeField]
		internal StateMachineState data;

		public IStateMachineState<CachedComponents> NextState(CachedComponents obj, int frameCount) => data.NextState(obj, frameCount);
		public void OnExit(CachedComponents obj, int frameCount) => data.OnExit(obj, frameCount);
		public void OnUpdate(CachedComponents obj, int frameCount) => data.OnUpdate(obj, frameCount);
	}
}

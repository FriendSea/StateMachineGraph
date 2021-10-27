using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea {
	public class StateMachineNodeAsset : ScriptableObject, IStateMachineState<GameObject>
	{
		[SerializeReference]
		StateMachineStateBase data;

		public IStateMachineState<GameObject> NextState(GameObject obj, int frameCount) => data.NextState(obj, frameCount);
		public void OnExit(GameObject obj, int frameCount) => data.OnExit(obj, frameCount);
		public void OnUpdate(GameObject obj, int frameCount) => data.OnUpdate(obj, frameCount);
	}
}

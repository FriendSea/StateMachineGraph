using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
    public class GameobjectStateMachine : MonoBehaviour
    {
        [SerializeField]
        StateMachineAsset asset;

        StateMachine<GameObject> stateMachine = null;

		private void Awake()
		{
			stateMachine = new StateMachine<GameObject>(asset.EntryState, asset.FallbackState, gameObject);
		}

		void FixedUpdate()
		{
			stateMachine.Update();
		}

		public void ForceState(StateMachineStateBase state)
		{
			stateMachine.ForceState(state);
		}
	}
}

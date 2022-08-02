using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	[RequireComponent(typeof(CachedComponents))]
    public class GameobjectStateMachine : MonoBehaviour
    {
        [SerializeField]
        StateMachineAsset asset;

		public StateMachine<CachedComponents> StateMachine => stateMachine;
        StateMachine<CachedComponents> stateMachine = null;

		private void Awake()
		{
			stateMachine = new StateMachine<CachedComponents>(asset.EntryState, asset.FallbackState, GetComponent<CachedComponents>());
		}

		void FixedUpdate()
		{
			stateMachine.Update();
		}

		public void ForceState(StateMachineNodeAsset state)
		{
			stateMachine.ForceState(state);
		}
	}
}

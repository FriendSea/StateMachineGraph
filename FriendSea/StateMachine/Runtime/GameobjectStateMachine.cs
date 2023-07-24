using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class GameobjectStateMachine : MonoBehaviour
    {
		public bool Paused { get; set; } = false;

        [SerializeField]
        StateMachineAsset asset;

		public StateMachine<IContextContainer> StateMachine => stateMachine;
        StateMachine<IContextContainer> stateMachine = null;

		private void Awake()
		{
			stateMachine = new StateMachine<IContextContainer>(asset.EntryState, asset.FallbackState, new GameObjectContextContainer(gameObject));
		}

		void FixedUpdate()
		{
			if (Paused) return;
			stateMachine.Update();
		}

		public void ForceState(NodeAsset state)
		{
			stateMachine.ForceState(state);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	public class GameObjectContextContainer : ContextContainerBase
	{
		GameObject obj;

		public GameObjectContextContainer(GameObject obj) => this.obj = obj;

		public override T Get<T>() where T : class
		{
			if (typeof(T) == typeof(GameObject)) return obj as T;
			if(typeof(Component).IsAssignableFrom(typeof(T)))
				if(!contextObjects.ContainsKey(typeof(T)))
					contextObjects.Add(typeof(T), obj.GetComponentInChildren<T>(true));
			return base.Get<T>();
		}
	}

	public class GameobjectStateMachine : MonoBehaviour
    {
		public event System.Action<GameobjectStateMachine> OnDestroyCalled;
		private void OnDestroy() => OnDestroyCalled?.Invoke(this);

		public bool Paused { get; set; } = false;

        [SerializeField]
        StateMachineAsset asset;

		public StateMachine<IContextContainer> StateMachine => stateMachine;
        StateMachine<IContextContainer> stateMachine = null;

		private void Awake()
		{
			var ctx = new GameObjectContextContainer(gameObject);
			stateMachine = new StateMachine<IContextContainer>(asset.EntryState, asset.FallbackState, ctx);
			ctx.Add(stateMachine);
		}

		void FixedUpdate()
		{
			if (Paused) return;
			stateMachine.Update(Time.fixedDeltaTime);
		}

		public void ForceState(NodeAsset state) =>
			stateMachine.ForceState(state);

		public void IssueTrigger(TriggerLabel label) =>
			Trigger.IssueTransiton(stateMachine, label);
	}
}

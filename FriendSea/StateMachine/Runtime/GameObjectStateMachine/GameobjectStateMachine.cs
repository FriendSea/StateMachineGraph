using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;
using System.Linq;

namespace FriendSea.StateMachine
{
	public class GameObjectContextContainer : ContextContainerBase
	{
		GameObject obj;
		public GameObjectContextContainer(GameObject obj) => this.obj = obj;

		public override T Get<T>() where T : class
		{
			if (typeof(T) == typeof(GameObject)) return obj as T;
			if (typeof(Component).IsAssignableFrom(typeof(T)) || typeof(T).IsInterface)
				if (!contextObjects.ContainsKey(typeof(T)))
					contextObjects.Add(typeof(T), obj.GetComponentInChildren<T>(true));
			return base.Get<T>();
		}
	}

	public class GameobjectStateMachine : MonoBehaviour
    {
		internal static event System.Action<GameobjectStateMachine> OnCreated;
		internal event System.Action<GameobjectStateMachine> OnDestroyCalled;
		private void OnDestroy() => OnDestroyCalled?.Invoke(this);

        [SerializeField]
        StateMachineAsset asset;

		public LayeredStateMachine<IContextContainer> StateMachine => stateMachine;
        LayeredStateMachine<IContextContainer> stateMachine = null;

		private void Awake() {
			var layers = asset.Layers.Select(l => new StateMachine<IContextContainer>(l.entry, l.fallback, new GameObjectContextContainer(gameObject)));
			stateMachine = new LayeredStateMachine<IContextContainer>(layers);

			OnCreated?.Invoke(this);
		}

		void FixedUpdate()
		{
			stateMachine.Update(Time.fixedDeltaTime);
		}

		public void ForceState(NodeAsset state) =>
			stateMachine.ForceState(state);

		public void IssueTrigger(TriggerLabel label) =>
			Trigger.IssueTransiton(stateMachine.DefaultLayer, label);
	}
}

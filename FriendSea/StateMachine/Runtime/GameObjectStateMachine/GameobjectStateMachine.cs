using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class GameObjectContextContainer : IContextContainer
	{
		GameObject obj;

		public GameObjectContextContainer(GameObject obj) => this.obj = obj;

		Dictionary<System.Type, Component> cache = new Dictionary<System.Type, Component>();
		public T Get<T>() where T : class
		{
			if (typeof(T) == typeof(GameObject)) return obj as T;
			if (!cache.ContainsKey(typeof(T)))
				cache.Add(typeof(T), obj.GetComponentInChildren<T>(true) as Component);
			return cache[typeof(T)] as T;
		}

		Dictionary<object, int> values = new Dictionary<object, int>();
		public int GetValue(object target)
		{
			if (!values.ContainsKey(target))
				values.Add(target, 0);
			return values[target];
		}
		public int SetValue(object target, int value) => values[target] = value;
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

using UnityEngine;
using FriendSea.StateMachine.Controls;
using System.Linq;
using System.Threading;


#if FSTATES_USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

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
#if FSTATES_USE_UNITASK
		[SerializeField]
		PlayerLoopTiming updateTiming = PlayerLoopTiming.FixedUpdate;
#else
		[SerializeField]
		bool useFixedUpdate = true;
#endif

		internal static event System.Action<GameobjectStateMachine> OnCreated;
		internal event System.Action<GameobjectStateMachine> OnDestroyCalled;
		private void OnDestroy() => OnDestroyCalled?.Invoke(this);

        [SerializeField]
        StateMachineAsset asset;

		public LayeredStateMachine StateMachine => stateMachine;
        LayeredStateMachine stateMachine = null;

		private void Awake() {
			var layers = asset.Layers.Select(l => new StateMachine<IContextContainer>(l.entry, l.fallback, new GameObjectContextContainer(gameObject)));
			stateMachine = new LayeredStateMachine(layers.ToList());

			OnCreated?.Invoke(this);

#if FSTATES_USE_UNITASK
			UpdateLoop(destroyCancellationToken).Forget();
#endif
		}

#if FSTATES_USE_UNITASK
		async UniTask UpdateLoop(CancellationToken cancellationToken)
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				stateMachine.Update(updateTiming is PlayerLoopTiming.FixedUpdate or PlayerLoopTiming.LastFixedUpdate ? Time.fixedDeltaTime : Time.time);
				await UniTask.Yield(updateTiming, cancellationToken);
			}
		}
#else
		void FixedUpdate()
		{
			if(useFixedUpdate)
				stateMachine.Update(Time.fixedDeltaTime);
		}
        private void Update()
        {
			if (!useFixedUpdate)
				stateMachine.Update(Time.deltaTime);
        }
#endif

        public void ForceState(NodeAsset state) =>
			stateMachine.ForceState(state);

		public void IssueTrigger(TriggerLabel label) =>
			Trigger.IssueTransiton(stateMachine.DefaultLayer, label);
	}
}

using System.Collections.Generic;
using System.Linq;

namespace FriendSea.StateMachine {
	public class StateMachine<T> where T : class
	{
		internal static event System.Action<StateMachine<T>> OnInstanceCreated;

		internal event System.Action<IState<T>> OnStateChanged;
		
		public IState<T> CurrentState { get; private set; }
		public IStateReference<T> FallbackState { get; private set; }
		T target;

		public StateMachine(IStateReference<T> entryState, IStateReference<T> fallbackState, T target)
		{
			CurrentState = entryState.GetState(null).state ?? fallbackState.GetState(null).state;
			FallbackState = fallbackState;
			this.target = target;

			CurrentState.OnEnter(target);

			OnInstanceCreated?.Invoke(this);
		}

		public void DoTransition()
		{
			var newstate = CurrentState.NextState(target);
			if (newstate != CurrentState)
				ForceState(newstate);
		}

		public void Update(float deltaTime)
		{
			// Transition
			DoTransition();
			// Update
			CurrentState.OnUpdate(target, deltaTime);
		}

		public void ForceState(IState<T> state)
		{
			state = state ?? FallbackState.GetState(target).state;

			CurrentState.OnExit(target);
			CurrentState = state;
			CurrentState.OnEnter(target);

			OnStateChanged?.Invoke(CurrentState);
		}

		public void ForceState(IStateReference<T> state) =>
			ForceState(state.GetState(target).state);
	}

	public interface IState<T> where T : class
	{
		IState<T> NextState(T obj);
		void OnEnter(T obj);
		void OnUpdate(T obj, float deltaTime);
		void OnExit(T obj);
		string Id { get; }
	}

	public interface IStateReference<T> where T : class
	{
		(IState<T> state, bool isValid) GetState(T obj);
	}
}

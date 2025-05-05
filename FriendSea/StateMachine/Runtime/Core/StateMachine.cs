#nullable enable

using System;

namespace FriendSea.StateMachine {
    public class StateMachine<T> : IDisposable where T : class
	{
		public event System.Action<IState<T>>? OnStateChanged;
		
		public IState<T> CurrentState { get; private set; }
		public IStateReference<T> FallbackState { get; private set; }
		public T Target { get; private set; }

		public StateMachine(IStateReference<T> entryState, IStateReference<T> fallbackState, T target)
		{
            if (fallbackState == null) throw new System.ArgumentNullException(nameof(fallbackState));
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            this.Target = target;
			CurrentState = entryState?.GetState(target).state ?? fallbackState.GetState(target).state;
			FallbackState = fallbackState;

			CurrentState.OnEnter(target);
		}

		public void DoTransition()
		{
			var newstate = CurrentState.NextState(Target) ?? FallbackState.GetState(Target).state;
            if (newstate != CurrentState)
				ForceState(newstate);
		}

		public void Update(float deltaTime)
		{
			DoTransition();
			CurrentState.OnUpdate(Target, deltaTime);
		}

		public void ForceState(IState<T> state)
		{
			state = state ?? FallbackState.GetState(Target).state;

			CurrentState.OnExit(Target);
			CurrentState = state;
			CurrentState.OnEnter(Target);

			OnStateChanged?.Invoke(CurrentState);
		}

		public void ForceState(IStateReference<T> state) =>
			ForceState(state.GetState(Target).state);

        public void Dispose() => CurrentState.OnExit(Target);
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

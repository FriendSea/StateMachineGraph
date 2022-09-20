using System.Collections;
using System.Collections.Generic;

namespace FriendSea.StateMachine {
	public class StateMachine<T> where T : class
	{
		public IState<T> CurrentState { get; private set; }
		public IStateReference<T> FallbackState { get; private set; }
		T target;

		public StateMachine(IStateReference<T> entryState, IStateReference<T> fallbackState, T target)
		{
			CurrentState = entryState.GetState(null, 0).state ?? fallbackState.GetState(null, 0).state;
			FallbackState = fallbackState;
			this.target = target;

			CurrentState.OnEnter(target, 0);
		}

		int frameCount = 0;
		public void Update()
		{
			var newstate = CurrentState.NextState(target, frameCount);
			if (newstate != CurrentState)
			{
				CurrentState.OnExit(target, frameCount);
				frameCount = 0;
				CurrentState = newstate ?? FallbackState.GetState(target, frameCount).state;
				CurrentState.OnEnter(target, frameCount);
			}
			CurrentState.OnUpdate(target, frameCount);
			frameCount++;
		}

		public void ForceState(IState<T> state)
		{
			CurrentState.OnExit(target, frameCount);
			CurrentState = state ?? FallbackState.GetState(target, frameCount).state;
			frameCount = 0;
			CurrentState.OnEnter(target, frameCount);
		}

		public void ForceState(IStateReference<T> state) =>
			ForceState(state.GetState(target, frameCount).state);
	}

	public interface IState<T> where T : class
	{
		IState<T> NextState(T obj, int frameCount);
		void OnEnter(T obj, int frameCount);
		void OnUpdate(T obj, int frameCount);
		void OnExit(T obj, int frameCount);
	}

	public interface IStateReference<T> where T : class
	{
		(IState<T> state, bool isValid) GetState(T obj, int frameCount);
	}
}

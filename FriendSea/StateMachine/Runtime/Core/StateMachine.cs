using System.Collections;
using System.Collections.Generic;

namespace FriendSea {
	public class StateMachine<T> where T : class
	{
		public IStateMachineState<T> CurrentState { get; private set; }
		public IStateMachineState<T> FallbackState { get; private set; }
		T target;

		public StateMachine(IStateMachineState<T> entryState, IStateMachineState<T> fallbackState, T target)
		{
			CurrentState = entryState;
			FallbackState = fallbackState;
			this.target = target;
		}

		int frameCount = 0;
		public void Update()
		{
			var newstate = CurrentState.NextState(target, frameCount);
			if (newstate != CurrentState)
			{
				CurrentState.OnExit(target, frameCount);
				frameCount = 0;
				CurrentState = newstate ?? FallbackState;
			}
			CurrentState.OnUpdate(target, frameCount);
			frameCount++;
		}

		public void ForceState(IStateMachineState<T> state)
		{
			CurrentState.OnExit(target, frameCount);
			CurrentState = state ?? FallbackState;
			frameCount = 0;
		}
	}

	public interface IStateMachineState<T> where T : class
	{
		IStateMachineState<T> NextState(T obj, int frameCount);
		void OnUpdate(T obj, int frameCount);
		void OnExit(T obj, int frameCount);
	}
}

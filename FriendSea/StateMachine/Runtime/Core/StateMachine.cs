using System.Collections;
using System.Collections.Generic;

namespace FriendSea {
	public class StateMachine<T> where T : class
	{
		class StateReference : IStateReference<T>
		{
			IStateMachineState<T> target;
			public StateReference(IStateMachineState<T> state) => target = state;

			public (IStateMachineState<T> state, bool isValid) GetState(T obj, int frameCount) => (target, true);
		}

		public IStateMachineState<T> CurrentState { get; private set; }
		public IStateReference<T> FallbackState { get; private set; }
		T target;

		public StateMachine(IStateReference<T> entryState, IStateReference<T> fallbackState, T target)
		{
			CurrentState = entryState.GetState(null, 0).state;
			FallbackState = fallbackState;
			this.target = target;
		}

		public StateMachine(IStateMachineState<T> entryState, IStateMachineState<T> fallbackState, T target) : this(new StateReference(fallbackState), new StateReference(fallbackState), target) { }

		int frameCount = 0;
		public void Update()
		{
			var newstate = CurrentState.NextState(target, frameCount);
			if (newstate != CurrentState)
			{
				CurrentState.OnExit(target, frameCount);
				frameCount = 0;
				CurrentState = newstate ?? FallbackState.GetState(target, frameCount).state;
			}
			CurrentState.OnUpdate(target, frameCount);
			frameCount++;
		}

		public void ForceState(IStateMachineState<T> state)
		{
			CurrentState.OnExit(target, frameCount);
			CurrentState = state ?? FallbackState.GetState(target, frameCount).state;
			frameCount = 0;
		}
	}

	public interface IStateMachineState<T> where T : class
	{
		IStateMachineState<T> NextState(T obj, int frameCount);
		void OnUpdate(T obj, int frameCount);
		void OnExit(T obj, int frameCount);
	}

	public interface IStateReference<T> where T : class
	{
		(IStateMachineState<T> state, bool isValid) GetState(T obj, int frameCount);
	}
}

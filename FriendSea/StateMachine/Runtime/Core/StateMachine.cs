using System.Collections;
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

		struct StatePairFrame
		{
			public int frameCount;
			public IState<T> state;
		}

		List<StatePairFrame> ResidentStates = new List<StatePairFrame>();

		public StateMachine(IStateReference<T> entryState, IStateReference<T> fallbackState, T target)
		{
			CurrentState = entryState.GetState(null, 0).state ?? fallbackState.GetState(null, 0).state;
			FallbackState = fallbackState;
			this.target = target;

			CurrentState.OnEnter(target, 0);
			foreach (var s in CurrentState.ResidentStates)
			{
				ResidentStates.Add(new StatePairFrame() { frameCount = 0, state = s });
				s.OnEnter(target, 0);
			}

			OnInstanceCreated?.Invoke(this);
		}

		int frameCount = 0;
		public void Update()
		{
			// Transition
			var newstate = CurrentState.NextState(target, frameCount);
			if (newstate != CurrentState)
				ForceState(newstate);
			// Update
			CurrentState.OnUpdate(target, frameCount);
			foreach (var s in ResidentStates)
				s.state.OnUpdate(target, s.frameCount);
			// add frame count;
			frameCount++;
			for (int i = 0; i < ResidentStates.Count; i++)
			{
				var pair = ResidentStates[i];
				pair.frameCount++;
				ResidentStates[i] = pair;
			}
		}

		public void ForceState(IState<T> state)
		{
			state = state ?? FallbackState.GetState(target, frameCount).state;

			// remove resident states
			CurrentState.OnExit(target, frameCount);
			for(int i = ResidentStates.Count - 1; i >= 0; i--)
			{
				if (state.ResidentStates.Contains(ResidentStates[i].state)) continue;
				ResidentStates[i].state.OnExit(target, ResidentStates[i].frameCount);
				ResidentStates.RemoveAt(i);
			}

			// replace main statge
			CurrentState = state;
			frameCount = 0;

			// add resident state
			CurrentState.OnEnter(target, frameCount);
			foreach (var s in state.ResidentStates)
			{
				if (ResidentStates.Select(pair => pair.state).Contains(s)) continue;
				ResidentStates.Add(new StatePairFrame() { frameCount = 0, state = s });
				s.OnEnter(target, 0);
			}

			OnStateChanged?.Invoke(CurrentState);
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
		IEnumerable<IState<T>> ResidentStates { get; }
		string Id { get; }
	}

	public interface IStateReference<T> where T : class
	{
		(IState<T> state, bool isValid) GetState(T obj, int frameCount);
	}
}

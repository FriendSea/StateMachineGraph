using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class ResidentState
	{
		[SerializeReference]
		internal IBehaviour[] behaviours = null;
		[SerializeField]
		internal Transition transition;
		[SerializeField]
		internal string id;
		public void OnEnter(IContextContainer ctx)
		{
			foreach (var b in behaviours)
				b.OnEnter(ctx);
		}
		public void OnExit(IContextContainer ctx)
		{
			foreach (var b in behaviours)
				b.OnExit(ctx);
		}
		public void OnUpdate(IContextContainer ctx)
		{
			foreach (var b in behaviours)
				b.OnUpdate(ctx);
		}
	}
	class ResidentStateContext
	{
		class ResidentStateContextContainer : IContextContainer
		{
			public ResidentStateContextContainer(IContextContainer ctx) => original = ctx;
			IContextContainer original;
			public int FrameCount { get; set; }
			public float Time { get; set; }
			public void Add<T>(T obj) where T : class => original.Add(obj);
			public T Get<T>() where T : class => original.Get<T>();
		}
		List<ResidentState> ResidentStates { get; } = new List<ResidentState>();
		Dictionary<ResidentState, ResidentStateContextContainer> contexts = new Dictionary<ResidentState, ResidentStateContextContainer>();

		ResidentStateContextContainer SetupContext(IContextContainer ctx, ResidentState resident)
		{
			if (!contexts.ContainsKey(resident))
				contexts.Add(resident, new ResidentStateContextContainer(ctx));
			contexts[resident].FrameCount = 0;
			contexts[resident].Time = 0;
			return contexts[resident];
		}

		public void ChangeResidents(IContextContainer ctx, IEnumerable<ResidentState> newResitdents)
		{
			for (int i = ResidentStates.Count - 1; i >= 0; i--)
			{
				if (newResitdents.Contains(ResidentStates[i])) continue;
				ResidentStates[i].OnExit(contexts[ResidentStates[i]]);
				ResidentStates.RemoveAt(i);
			}

			foreach (var s in newResitdents)
			{
				if (ResidentStates.Contains(s)) continue;
				ResidentStates.Add(s);
				s.OnEnter(SetupContext(ctx, s));
			}
		}

		public void UpdateResidents(IContextContainer ctx, float deltaTime)
		{
			foreach (var state in ResidentStates)
				state.OnUpdate(contexts[state]);
			foreach (var c in contexts.Values)
			{
				c.Time += deltaTime;
				c.FrameCount++;
			}
		}
		public (IState<IContextContainer> state, bool isValid) GetTransition(IContextContainer ctx)
		{
			foreach (var state in ResidentStates)
			{
				var result = state.transition.GetState(contexts[state]);
				if (result.isValid)
					return result;
			}
			return (null, false);
		}
	}

	[System.Serializable]
	internal struct ResitentStateRefernce : IEnumerable<ResidentState>
	{
		[SerializeField]
		internal StateMachineAsset stateMachine;
		[SerializeField]
		internal string[] guids;
		[System.NonSerialized]
		List<ResidentState> cachedList;

		public IEnumerator<ResidentState> GetEnumerator()
		{
			if (cachedList == null)
			{
				cachedList = new List<ResidentState>();
				foreach (var guid in guids)
					cachedList.Add(stateMachine.GetResidentState(guid));
			}
			return cachedList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

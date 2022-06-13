using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	[System.Serializable]
	public sealed class StateMachineState : IStateMachineState<CachedComponents>
	{
		public interface IBehaviour
		{
			void OnUpdate(CachedComponents obj, int frameCount);
			void OnExit(CachedComponents obj, int frameCount);
		}

		public interface ITransition
		{
			bool ShouldTransition(CachedComponents obj, int frameCount);
		}

		[SerializeReference]
		internal IBehaviour[] behaviours = null;

		[System.Serializable]
		internal struct Transition
		{
			[SerializeField]
			public StateMachineNodeAsset target;
			[SerializeReference]
			public ITransition[] conditions;

			public bool ShouldTransition(CachedComponents obj, int frameCount)
			{
				foreach(var t in conditions)
					if (!t.ShouldTransition(obj, frameCount)) return false;
				return true;
			}
		}

		[SerializeField]
		internal Transition[] transitions;

		public IStateMachineState<CachedComponents> NextState(CachedComponents obj, int frameCount)
		{
			foreach (var t in transitions)
				if (t.ShouldTransition(obj, frameCount))
					return t.target;
			return this;
		}

		public void OnExit(CachedComponents obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnExit(obj, frameCount);
		}

		public void OnUpdate(CachedComponents obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnUpdate(obj, frameCount);
		}
	}
}

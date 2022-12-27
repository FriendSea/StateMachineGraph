using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine
{
	public class ImmediateTransition : State.Transition.ICondition
	{
		public bool IsValid(CachedComponents obj, int frameCount) => true;
	}

	public interface IGameObjectState : IState<CachedComponents> { }

	[System.Serializable]
	public class State : IGameObjectState
	{
		public interface IBehaviour
		{
			void OnEnter(CachedComponents obj, int frameCount);
			void OnUpdate(CachedComponents obj, int frameCount);
			void OnExit(CachedComponents obj, int frameCount);
		}

		public interface IStateReference : IStateReference<CachedComponents> { }

		[System.Serializable]
		public struct StateReference : IStateReference
		{
			[SerializeField]
			internal NodeAsset nodeAsset;

			public (IState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount) => nodeAsset.GetState(obj, frameCount);
		}

		[SerializeReference]
		internal IBehaviour[] behaviours = null;

		[System.Serializable]
		public struct Transition : IStateReference
		{
			public interface ICondition
			{
				bool IsValid(CachedComponents obj, int frameCount);
			}

			[SerializeReference]
			public ICondition condition;
			[SerializeReference]
			public IStateReference[] targets;

			public (IState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount)
			{
				// 条件にマッチしてない。遷移しない。
				if (!condition.IsValid(obj, frameCount)) return (null, false);
				// マッチしたが遷移先がない、nullに遷移
				if (targets.Length <= 0) return (null, true);

				// 接続先の最初の有効なものに遷移
				foreach(var target in targets)
				{
					var result = target.GetState(obj, frameCount);
					if (result.isValid) return (result.state, true);
				}
				// 何も有効じゃなかった。遷移しない。
				return (null, false);
			}
		}

		[SerializeField]
		internal Transition transition;

		[System.Serializable]
		public struct ResitentStateRefernce : IEnumerable<IState<CachedComponents>>
		{
			[SerializeField]
			internal StateMachineAsset stateMachine;
			[SerializeField]
			internal string[] guids;
			[System.NonSerialized]
			List<IState<CachedComponents>> cachedList;

			public IEnumerator<IState<CachedComponents>> GetEnumerator()
			{
				if (cachedList == null)
				{
					cachedList = new List<IState<CachedComponents>>();
					foreach (var guid in guids)
						cachedList.Add(stateMachine.GetResidentState(guid));
				}
				return cachedList.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[SerializeField]
		internal ResitentStateRefernce residentStates;

		public IState<CachedComponents> NextState(CachedComponents obj, int frameCount)
		{
			var result = transition.GetState(obj, frameCount);
			return result.isValid ?
				result.state :
				this;
		}

		public void OnEnter(CachedComponents obj, int frameCount)
		{
			foreach (var b in behaviours)
				b.OnEnter(obj, frameCount);
		}

		public void OnExit(CachedComponents obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnExit(obj, frameCount);
		}

		public void OnUpdate(CachedComponents obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnUpdate(obj, frameCount);
		}

		public IEnumerable<IState<CachedComponents>> ResidentStates => residentStates;
	}
}
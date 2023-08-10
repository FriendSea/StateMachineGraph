using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine
{
	public class ImmediateTransition : State.Transition.ICondition
	{
		public bool IsValid(IContextContainer obj, int frameCount) => true;
	}

	public interface IGameObjectState : IState<IContextContainer> { }

	[System.Serializable]
	public class State : IGameObjectState
	{
		public interface IBehaviour
		{
			void OnEnter(IContextContainer obj, int frameCount);
			void OnUpdate(IContextContainer obj, int frameCount);
			void OnExit(IContextContainer obj, int frameCount);
		}

		public interface IStateReference : IStateReference<IContextContainer> { }

		[System.Serializable]
		public struct StateReference : IStateReference
		{
			[SerializeField]
			internal NodeAsset nodeAsset;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj, int frameCount) => nodeAsset.GetState(obj, frameCount);
		}

		[System.Serializable]
		public class Sequence : IStateReference
		{
			[SerializeReference]
			public IStateReference[] targets;
			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj, int frameCount)
			{
				// 遷移先がない、nullに遷移
				if (targets.Length <= 0) return (null, true);

				var currentIndex = obj.GetValue(this);
				obj.SetValue(this, (currentIndex + 1) % targets.Length);

				// 現在のインデックスの遷移先
				var result = targets[currentIndex].GetState(obj, frameCount);
				if (result.isValid) 
					return (result.state, true);
				else
					return (null, true);
			}
		}

		[SerializeReference]
		internal IBehaviour[] behaviours = null;

		[System.Serializable]
		public struct Transition : IStateReference
		{
			public interface ICondition
			{
				bool IsValid(IContextContainer obj, int frameCount);
			}

			[SerializeReference]
			public ICondition condition;
			[SerializeReference]
			public IStateReference[] targets;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj, int frameCount)
			{
				// 遷移なし
				if (condition == null || targets == null) return (null, false);
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
		public struct ResitentStateRefernce : IEnumerable<IState<IContextContainer>>
		{
			[SerializeField]
			internal StateMachineAsset stateMachine;
			[SerializeField]
			internal string[] guids;
			[System.NonSerialized]
			List<IState<IContextContainer>> cachedList;

			public IEnumerator<IState<IContextContainer>> GetEnumerator()
			{
				if (cachedList == null)
				{
					cachedList = new List<IState<IContextContainer>>();
					foreach (var guid in guids)
						cachedList.Add(stateMachine.GetResidentState(guid));
				}
				return cachedList.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[SerializeField]
		internal ResitentStateRefernce residentStates;

		public IState<IContextContainer> NextState(IContextContainer obj, int frameCount)
		{
			var result = transition.GetState(obj, frameCount);
			return result.isValid ?
				result.state :
				this;
		}

		public void OnEnter(IContextContainer obj, int frameCount)
		{
			foreach (var b in behaviours)
				b.OnEnter(obj, frameCount);
		}

		public void OnExit(IContextContainer obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnExit(obj, frameCount);
		}

		public void OnUpdate(IContextContainer obj, int frameCount) {
			foreach (var b in behaviours)
				b.OnUpdate(obj, frameCount);
		}

		public IEnumerable<IState<IContextContainer>> ResidentStates => residentStates;

		[SerializeField]
		internal string id;
		public string Id => id;
	}
}

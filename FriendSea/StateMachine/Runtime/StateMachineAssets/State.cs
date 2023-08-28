using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine
{
	[DisplayName("Hidden/Always")]
	public class ImmediateTransition : State.Transition.ICondition
	{
		public bool IsValid(IContextContainer obj) => true;
	}

	public interface IGameObjectState : IState<IContextContainer> { }

	[System.Serializable]
	public class State : IGameObjectState
	{
		public interface IBehaviour
		{
			void OnEnter(IContextContainer obj);
			void OnUpdate(IContextContainer obj);
			void OnExit(IContextContainer obj);
		}

		public interface IStateReference : IStateReference<IContextContainer> { }

		[System.Serializable]
		public struct StateReference : IStateReference
		{
			[SerializeField]
			internal NodeAsset nodeAsset;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj) => nodeAsset.GetState(obj);
		}

		[System.Serializable]
		public class Sequence : IStateReference
		{
			class Context {
				Dictionary<object, int> values = new Dictionary<object, int>();
				public int GetValue(object target)
				{
					if (!values.ContainsKey(target))
						values.Add(target, 0);
					return values[target];
				}
				public int SetValue(object target, int value) => values[target] = value;
			}

			[SerializeReference]
			public IStateReference[] targets;
			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
			{
				// 遷移先がない、nullに遷移
				if (targets.Length <= 0) return (null, true);

				var indexContext = obj.GetOrCreate<Context>();
				var currentIndex = indexContext.GetValue(this);
				indexContext.SetValue(this, (currentIndex + 1) % targets.Length);

				// 現在のインデックスの遷移先
				var result = targets[currentIndex].GetState(obj);
				return result.isValid ?
					(result.state, true) :
					(null, true);
			}
		}

		[System.Serializable]
		public class Random : IStateReference
		{
			[SerializeReference]
			public IStateReference[] targets;
			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
			{
				// 遷移先がない、nullに遷移
				if (targets.Length <= 0) return (null, true);

				// ランダム遷移
				return targets[UnityEngine.Random.Range(0, targets.Length)].GetState(obj);
			}
		}

		public class Trigger : IStateReference
		{
			static List<TriggerLabel> activeLabels = new List<TriggerLabel>();
			public static void IssueTransiton<T>(StateMachine<T> stateMachine, TriggerLabel label) where T : class
			{
				activeLabels.Add(label);
				stateMachine.DoTransition();
				activeLabels.Remove(label);
			}

			[SerializeField]
			internal TriggerLabel label;
			[SerializeReference]
			public IStateReference[] targets;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
			{
				foreach (var target in targets)
				{
					var result = target.GetState(obj);
					if (result.isValid) return (result.state, activeLabels.Contains(label));
				}
				return (null, activeLabels.Contains(label));
			}
		}

		[SerializeReference]
		internal IBehaviour[] behaviours = null;

		[System.Serializable]
		public struct Transition : IStateReference
		{
			public interface ICondition
			{
				bool IsValid(IContextContainer obj);
			}

			[SerializeReference]
			public ICondition condition;
			[SerializeReference]
			public IStateReference[] targets;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
			{
				// 遷移なし
				if (condition == null || targets == null) return (null, false);
				// 条件にマッチしてない。遷移しない。
				if (!condition.IsValid(obj)) return (null, false);
				// マッチしたが遷移先がない、nullに遷移
				if (targets.Length <= 0) return (null, true);

				// 接続先の最初の有効なものに遷移
				foreach(var target in targets)
				{
					var result = target.GetState(obj);
					if (result.isValid) return (result.state, true);
				}
				// 何も有効じゃなかった。遷移しない。
				return (null, false);
			}
		}

		[SerializeField]
		internal Transition transition;

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

		[SerializeField]
		internal ResitentStateRefernce residentStates;

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
			List<ResidentState> ResidentStates { get; } = new List<ResidentState>();

			public void ChangeResidents(IContextContainer ctx, IEnumerable<ResidentState> newResitdents)
			{
				for (int i = ResidentStates.Count - 1; i >= 0; i--)
				{
					if (newResitdents.Contains(ResidentStates[i])) continue;
					ResidentStates[i].OnExit(ctx);
					ResidentStates.RemoveAt(i);
				}

				foreach (var s in newResitdents)
				{
					if (ResidentStates.Contains(s)) continue;
					ResidentStates.Add(s);
					s.OnEnter(ctx);
				}
			}

			public void UpdateResidents(IContextContainer ctx)
			{
				foreach (var state in ResidentStates)
					state.OnUpdate(ctx);
			}
		}

		public IState<IContextContainer> NextState(IContextContainer obj)
		{
			var result = transition.GetState(obj);
			if (result.isValid)
				return result.state;
			foreach (var resident in residentStates)
			{
				var residentResult = resident.transition.GetState(obj);
				if (residentResult.isValid)
					return residentResult.state;
			}
			return this;
		}

		public void OnEnter(IContextContainer obj)
		{
			obj.FrameCount = 0;
			obj.Time = 0f;
			obj.GetOrCreate<ResidentStateContext>().ChangeResidents(obj, residentStates);
			foreach (var b in behaviours)
				b.OnEnter(obj);
		}

		public void OnExit(IContextContainer obj) {
			foreach (var b in behaviours)
				b.OnExit(obj);
		}

		public void OnUpdate(IContextContainer obj, float deltaTime) {
			foreach (var b in behaviours)
				b.OnUpdate(obj);
			obj.GetOrCreate<ResidentStateContext>().UpdateResidents(obj);
			obj.FrameCount++;
			obj.Time += deltaTime;
		}

		[SerializeField]
		internal string id;
		public string Id => id;
	}
}

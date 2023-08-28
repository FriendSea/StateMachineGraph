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

		public IState<IContextContainer> NextState(IContextContainer obj)
		{
			var result = transition.GetState(obj);
			if (result.isValid)
				return result.state;

			var residentsTransition = obj.GetOrCreate<ResidentStateContext>().GetTransition(obj);
			if (residentsTransition.isValid)
				return residentsTransition.state;

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
			obj.GetOrCreate<ResidentStateContext>().UpdateResidents(obj, deltaTime);
			obj.FrameCount++;
			obj.Time += deltaTime;
		}

		[SerializeField]
		internal string id;
		public string Id => id;
	}
}

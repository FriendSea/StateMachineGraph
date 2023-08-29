using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	public interface IBehaviour
	{
		void OnEnter(IContextContainer obj);
		void OnUpdate(IContextContainer obj);
		void OnExit(IContextContainer obj);
	}

	public interface ISerializableState : IState<IContextContainer> { }
	public interface ISerializableStateReference : IStateReference<IContextContainer> { }

	[System.Serializable]
	public class State : ISerializableState
	{
		[System.Serializable]
		public struct StateReference : ISerializableStateReference
		{
			[SerializeField]
			internal NodeAsset nodeAsset;

			public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj) => nodeAsset.GetState(obj);
		}

		[SerializeReference]
		internal IBehaviour[] behaviours = null;

		[SerializeField]
		internal Transition transition;
		[SerializeField]
		internal ResitentStateRefernce residentStates;

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

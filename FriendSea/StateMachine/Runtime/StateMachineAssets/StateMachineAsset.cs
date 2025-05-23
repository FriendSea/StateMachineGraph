using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;
using System.Linq;
using System;

namespace FriendSea.StateMachine
{
	public class StateMachineAsset : ScriptableObject
	{
		[Serializable]
		internal struct Layer
		{
			[SerializeField]
			public Transition entryState;
			[SerializeReference]
			public ISerializableStateReference fallbackState;
		}

		[SerializeField]
		internal Layer[] layers;
		[SerializeField]
		internal ResidentState[] residentStates;

		internal ResidentState GetResidentState(string guid)
		{
			foreach(var state in residentStates)
				if (state.id == guid) return state;
			return null;
		}

		IEnumerable<(IStateReference<IContextContainer> entry, IStateReference<IContextContainer> fallback)> Layers => layers.Select(l => ((IStateReference<IContextContainer>)l.entryState, (IStateReference<IContextContainer>)l.fallbackState));

		public LayeredStateMachine CreateStateMachineInstance(IContextContainer context)
		{
			var instance = new LayeredStateMachine(Layers, context);
			InstanceCreated?.Invoke(this, instance);
			return instance;
		}

		public static event Action<StateMachineAsset, LayeredStateMachine> InstanceCreated;
	}
}
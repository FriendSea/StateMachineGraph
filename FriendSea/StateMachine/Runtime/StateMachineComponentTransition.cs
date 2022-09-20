using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public struct StateMachineComponentTransition : StateMachineState.IStateReference
	{
		static List<StateMachineState.IStateReference> sharedList = new List<StateMachineState.IStateReference>();

		public (IStateMachineState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount)
		{
			sharedList.Clear();
			obj.gameObject.GetComponentsInChildren(sharedList);
			foreach (var reference in sharedList)
			{
				var result = reference.GetState(obj, frameCount);
				if (result.isValid)
					return result;
			}
			return (null, false);
		}
	}
}

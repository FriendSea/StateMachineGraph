using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Controls
{
	public class EmbededStateReference : ISerializableStateReference
	{
		public static EmbededStateLabel CurrentLabel { get; private set; }

		[SerializeField]
		internal EmbededStateLabel label;

		static List<ISerializableStateReference> sharedList = new List<ISerializableStateReference>();

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			CurrentLabel = label;
			obj.Get<GameObject>().GetComponentsInChildren(true, sharedList);
			foreach(var reference in sharedList)
			{
				var result = reference.GetState(obj);
				if (result.isValid)
					return result;
			}
			return (null, false);
		}
	}
}

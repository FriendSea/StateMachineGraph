using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class DisplayNameAttribute
#if UNITY_EDITOR
		: System.ComponentModel.DisplayNameAttribute
#endif
	{
		public DisplayNameAttribute(string name)
#if UNITY_EDITOR
			: base(name)
#endif
		{ }
	}
}

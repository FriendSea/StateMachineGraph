using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("FrameSpan")]
	partial class FrameSpan : IBehaviour
    {
		[SerializeField]
		int start;
		[SerializeField]
		int end;
		[SerializeReference]
		IBehaviour behaviour;
		public void OnEnter(IContextContainer obj) { }

		public void OnExit(IContextContainer obj)
		{
			if (obj.FrameCount > end + 1) return;
			behaviour.OnExit(obj);
		}

		public void OnUpdate(IContextContainer obj)
		{
			if (obj.FrameCount == start)
				behaviour.OnEnter(obj);
			if (obj.FrameCount >= start && obj.FrameCount <= end)
				behaviour.OnUpdate(obj);
			if (obj.FrameCount == end + 1)
				behaviour.OnExit(obj);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("FrameSpan")]
	partial class FrameSpan : BehaviourBase
	{
		[SerializeField]
		int start;
		[SerializeField]
		int end;
		[SerializeReference]
		IBehaviour behaviour;
		protected override void OnEnter(IContextContainer obj) { }

		protected override void OnExit(IContextContainer obj)
		{
			if (obj.FrameCount > end + 1) return;
			behaviour.OnExit(obj);
		}

		protected override void OnUpdate(IContextContainer obj)
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

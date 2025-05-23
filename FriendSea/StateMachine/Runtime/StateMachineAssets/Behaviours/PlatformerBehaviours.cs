using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	public interface IDirectionable
	{
		int Direction { set; }
	}

    [DisplayName("Platformer/SetDirection")]
    partial class PlatformerSetDirection : IBehaviour
    {
        [InjectContext]
        IDirectionable directionable;
        [SerializeField]
        int direction;

        public void OnEnter(IContextContainer obj) => directionable.Direction = direction;

        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj) { }
    }
}

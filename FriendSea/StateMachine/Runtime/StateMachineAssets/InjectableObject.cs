using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
    public interface IInjectable
    {
        void OnSetup(FriendSea.StateMachine.IContextContainer ctx);
    }
}

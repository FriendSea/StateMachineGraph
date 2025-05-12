using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
    public class SingleStateRunner : MonoBehaviour
    {
        [SerializeField]
        bool useFixedUpdate;
        [SerializeField]
        State state;

        GameObjectContextContainer context;

        private void OnEnable()
        {
            context ??= new GameObjectContextContainer(gameObject);
            state.OnEnter(context);
        }

        private void Update()
        {
            if (useFixedUpdate) return;
            state.OnUpdate(context, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if(!useFixedUpdate) return;
            state.OnUpdate(context, Time.fixedDeltaTime);
        }


        private void OnDisable() =>
            state.OnExit(context);
    }
}

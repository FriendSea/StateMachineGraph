#if STATEMACHINE_USE_INPUTSYSTEM

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Collections.Specialized.BitVector32;

namespace FriendSea.StateMachine.Behaviours
{
    [DisplayName("Movements/Input/AddVelocity")]
    partial class AddMovementVelocityFromInput : IBehaviour
    {
        [SerializeField]
        float factor = 1;
        [SerializeField]
        InputActionProperty input;
        [InjectContext]
        IMovement movement;

        public void OnEnter(IContextContainer obj)
        {
            input.action.Enable();
        }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            var vec = (Vector3)input.action.ReadValue<Vector2>();
            movement.Velocity += vec * factor;
        }
    }

    [DisplayName("Movements/Input/AddHorizontalVelocity")]
    partial class AddMovementHorizontalVelocityFromInput : IBehaviour
    {
        [SerializeField]
        float factor = 1;
        [SerializeField]
        InputActionProperty input;
        [InjectContext]
        IMovement movement;

        public void OnEnter(IContextContainer obj)
        {
            if (!input.action.enabled)
                input.action.Enable();
        }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            var val = (float)input.action.ReadValue<float>();
            movement.Velocity += Vector3.right * val * factor;
        }
    }

    [DisplayName("Movements/Input/AddVerticalVelocity")]
    partial class AddMovementVerticalVelocityFromInput : IBehaviour
    {
        [SerializeField]
        float factor = 1;
        [SerializeField]
        InputActionProperty input;
        [InjectContext]
        IMovement movement;

        public void OnEnter(IContextContainer obj)
        {
            if (!input.action.enabled)
                input.action.Enable();
        }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            var val = (float)input.action.ReadValue<float>();
            movement.Velocity += Vector3.up * val * factor;
        }
    }

    [DisplayName("Movements/Input/AddPlanerVelocity")]
    partial class AddMovementPlanerVelocityFromInput : IBehaviour
    {
        [SerializeField]
        float factor = 1;
        [SerializeField]
        InputActionProperty input;
        [InjectContext]
        IMovement movement;

        public void OnEnter(IContextContainer obj)
        {
            if (!input.action.enabled)
                input.action.Enable();
        }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            var val = input.action.ReadValue<Vector2>();
            var vec = Camera.main.transform.rotation * new Vector3(val.x, 0, val.y);
            vec.y = 0;
            movement.Velocity += vec.normalized * Mathf.Max(val.magnitude, 1f) * factor;
        }
    }

    [DisplayName("Movements/Input/PlanerRotate")]
    partial class SetPlanerRotationFromInput : IBehaviour
    {
        [SerializeField, Range(0.01f, 1)]
        float lerp = 1;
        [SerializeField]
        InputActionProperty input;
        [InjectContext]
        Transform transform;

        public void OnEnter(IContextContainer obj)
        {
            if (!input.action.enabled)
                input.action.Enable();
        }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            var val = input.action.ReadValue<Vector2>();
            if (Mathf.Approximately(0, val.sqrMagnitude)) return;
            var vec = Camera.main.transform.rotation * new Vector3(val.x, 0, val.y);
            vec.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vec.normalized), lerp);
        }
    }
}

#endif

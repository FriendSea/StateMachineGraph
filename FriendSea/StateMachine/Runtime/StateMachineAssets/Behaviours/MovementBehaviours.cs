using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	public interface IMovement
	{
		public Vector3 Velocity { get; set; }
	}

	[DisplayName("Movements/AddVelocity")]
	partial class AddMovementVelocity : IBehaviour
    {
		[SerializeField]
		Vector3 amount;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj) { }

		public void OnExit(IContextContainer obj) { }

		public void OnUpdate(IContextContainer obj)
		{
			movement.Velocity += amount;
		}
	}

	[DisplayName("Movements/MultiplyVelocity")]
	partial class MultiplyMovementVelocity : IBehaviour
	{
		[SerializeField]
		Vector3 factor;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj) { }

		public void OnExit(IContextContainer obj) { }

		public void OnUpdate(IContextContainer obj)
		{
			movement.Velocity = Vector3.Scale(movement.Velocity, factor);
		}
	}

    [DisplayName("Movements/LerpPosition")]
    partial class LerpMovementPosition : IBehaviour
    {
		[SerializeField, Range(0.01f, 1f)]
		float lerp = 1f;
		[InjectContext]
		Transform transform;
        [InjectContext]
        IMovement movement;
		[SerializeField]
		Transform target;

        public void OnEnter(IContextContainer obj) { }

        public void OnExit(IContextContainer obj) { }

        public void OnUpdate(IContextContainer obj)
        {
			var newPos = Vector3.Lerp(transform.position, target.position, lerp);
			movement.Velocity = (newPos - transform.position) / Time.fixedDeltaTime;
        }
    }

    [DisplayName("Transform/LerpRotation")]
    partial class LerpMovementRotation : IBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        float lerp = 1f;
        [InjectContext]
        Transform transform;
        [SerializeField]
        Transform target;

        public void OnEnter(IContextContainer obj) { }

        public void OnExit(IContextContainer obj) { }

        public void OnUpdate(IContextContainer obj)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, lerp);
        }
    }

    [DisplayName("Transform/LookAt")]
    partial class LookAt : IBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        float lerp = 1f;
        [InjectContext]
        Transform transform;
        [SerializeField]
        Transform target;

        public void OnEnter(IContextContainer obj) { }

        public void OnExit(IContextContainer obj) { }

        public void OnUpdate(IContextContainer obj)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), lerp);
        }
    }
}

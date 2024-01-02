using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DBridge : MonoBehaviour, FriendSea.StateMachine.Behaviours.IMovement, FriendSea.StateMachine.Conditions.IPlatformerObject
{
	[SerializeField]
	float groundHeight = -1f;
	[SerializeField]
	float groundMargin = 0.1f;

	Rigidbody2D _target;
	Rigidbody2D Target => _target ?? (_target = GetComponent<Rigidbody2D>());

	Vector3 _velocity;

	public Vector3 Velocity
	{
		get => _velocity;
		set => _velocity = value;
	}

	ContactPoint2D[] contacts = new ContactPoint2D[1];
	private void FixedUpdate()
	{
		var delta = Target.position + (Vector2)Velocity * Time.fixedDeltaTime;
		var groundDist = CheckGround();
		if (!float.IsNaN(groundDist))
			delta += -(Vector2)transform.up * groundDist;
		Target.MovePosition(delta);
		Target.velocity = Vector3.zero;
	}

	public bool IsGrounded => !float.IsNaN(CheckGround());

	RaycastHit2D[] results = new RaycastHit2D[4];
	float CheckGround()
	{
		var from = transform.position + transform.up * groundHeight + transform.up * groundMargin;
		var to = transform.position + transform.up * groundHeight - transform.up * groundMargin;
		var count = Physics2D.Raycast(from, (to - from), new ContactFilter2D(), results, (to - from).magnitude);
		for(int i = 0; i < count; i++) {
			if (results[i].collider.gameObject == gameObject) continue;
			return results[i].distance - groundMargin;
		}
		return float.NaN;
	}
}

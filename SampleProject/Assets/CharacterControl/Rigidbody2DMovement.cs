using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DMovement : MonoBehaviour, FriendSea.StateMachine.Behaviours.IMovement, FriendSea.StateMachine.Conditions.IPlatformerObject
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
		for (int i = 0; i < count; i++)
		{
			if (results[i].collider.gameObject == gameObject) continue;
			return results[i].distance - groundMargin;
		}
		return float.NaN;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.up * groundHeight, new Vector3(1, 0, 1));
		Gizmos.DrawLine(Vector3.up * (+groundMargin + groundHeight), Vector3.up * (-groundMargin + groundHeight));
		Gizmos.matrix = Matrix4x4.identity;
	}
}

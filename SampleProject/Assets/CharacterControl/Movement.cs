using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	[SerializeField]
	Vector3 collisionPoint0;
	[SerializeField]
	Vector3 collisionPoint1;
	[SerializeField]
	float collisionRadius = 0.5f;

	Vector3 GlobalCollisionPoint0 => transform.position + transform.rotation * collisionPoint0;
	Vector3 GlobalCollisionPoint1 => transform.position + transform.rotation * collisionPoint1;

	[field:SerializeField]
	public Vector3 Velocity { get; set; }

	private void Update()
	{
		UpdatePosition(Time.deltaTime);
	}

	void UpdatePosition(float deltaTime)
	{
		if (Velocity == Vector3.zero) return;
		var delta = Velocity * deltaTime;
		var isHit = Physics.CapsuleCast(GlobalCollisionPoint0 - delta, GlobalCollisionPoint1 - delta, collisionRadius, delta.normalized, out RaycastHit hit, delta.magnitude * 2f);
		var intersectLength = isHit ?
			delta.magnitude * 2f - hit.distance :
			0f;

		delta += hit.normal * Vector3.Dot(hit.normal, -delta.normalized) * intersectLength;

		transform.position += delta;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(GlobalCollisionPoint0, collisionRadius);
		Gizmos.DrawWireSphere(GlobalCollisionPoint1, collisionRadius);
	}
}

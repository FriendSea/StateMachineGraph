using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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

	RaycastHit[] hits = new RaycastHit[4];
	void UpdatePosition(float deltaTime)
	{
		if (Velocity == Vector3.zero) return;
		var delta = Velocity * deltaTime;
		var modDelta = delta;

		var hitcount = Physics.CapsuleCastNonAlloc(GlobalCollisionPoint0 - delta, GlobalCollisionPoint1 - delta, collisionRadius, delta.normalized, hits, delta.magnitude * 2f);
		for(int i = 0; i < hitcount; i++)
		{
			if (Mathf.Approximately(hits[i].distance, 0f)) continue;
			var intersectLength = delta.magnitude * 2f - hits[i].distance;
			Debug.Log(hits[i].normal);
			modDelta += hits[i].normal * Vector3.Dot(hits[i].normal, -delta.normalized) * intersectLength;
		}

		transform.position += modDelta;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(GlobalCollisionPoint0, collisionRadius);
		Gizmos.DrawWireSphere(GlobalCollisionPoint1, collisionRadius);
	}
}

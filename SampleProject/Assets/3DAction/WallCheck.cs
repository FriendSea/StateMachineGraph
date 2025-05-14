using FriendSea.StateMachine;
using FriendSea.StateMachine.Controls;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    [SerializeField]
    Vector3 rayPoint;
    [SerializeField]
    Vector3 rayDirection;
    [SerializeField]
    LayerMask layerMask;

    public bool IsObstacleAhead { get; private set; }

    private void FixedUpdate()
    {
        IsObstacleAhead = Physics.Raycast(transform.position + transform.rotation * rayPoint, transform.rotation * rayDirection, rayDirection.magnitude, layerMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + transform.rotation * rayPoint, transform.position + transform.rotation * (rayPoint + rayDirection));
    }
}

[DisplayName("Sample/IsObstacleAhead")]
partial class IsObstacleAhead : Transition.ICondition
{
    [InjectContext]
    WallCheck wallCheck;
    public bool IsValid(IContextContainer obj) => wallCheck.IsObstacleAhead;
}

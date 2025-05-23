using FriendSea.StateMachine;
using FriendSea.StateMachine.Controls;
using UnityEngine;

namespace FriendSea.StateMachine.Conditions
{
    public class WallCheck : MonoBehaviour
    {
        [SerializeField]
        Vector3 rayPoint;
        [SerializeField]
        float length;
        [SerializeField]
        LayerMask layerMask;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(rayPoint, length);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Vector3 RayPoint => transform.position + transform.rotation * rayPoint;
        public bool IsObstacleAhead => Physics.Raycast(RayPoint, transform.forward, length, layerMask, QueryTriggerInteraction.Ignore);
        public bool IsObstacleBack => Physics.Raycast(RayPoint, -transform.forward, length, layerMask, QueryTriggerInteraction.Ignore);
        public bool IsObstacleLeft => Physics.Raycast(RayPoint, -transform.right, length, layerMask, QueryTriggerInteraction.Ignore);
        public bool IsObstacleRight => Physics.Raycast(RayPoint, transform.right, length, layerMask, QueryTriggerInteraction.Ignore);
    }

    [DisplayName("Platformer/IsObstacleAhead")]
    partial class IsObstacleAhead : Transition.ICondition
    {
        [InjectContext]
        WallCheck wallCheck;
        public bool IsValid(IContextContainer obj) => wallCheck.IsObstacleAhead;
    }
    [DisplayName("Platformer/IsObstacleBehind")]
    partial class IsObstacleBehind : Transition.ICondition
    {
        [InjectContext]
        WallCheck wallCheck;
        public bool IsValid(IContextContainer obj) => wallCheck.IsObstacleBack;
    }
    [DisplayName("Platformer/IsObstacleLeft")]
    partial class IsObstacleLeft : Transition.ICondition
    {
        [InjectContext]
        WallCheck wallCheck;
        public bool IsValid(IContextContainer obj) => wallCheck.IsObstacleLeft;
    }
    [DisplayName("Platformer/IsObstacleRight")]
    partial class IsObstacleRight : Transition.ICondition
    {
        [InjectContext]
        WallCheck wallCheck;
        public bool IsValid(IContextContainer obj) => wallCheck.IsObstacleRight;
    }
}

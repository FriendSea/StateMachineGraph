using FriendSea.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public partial class IntaractableObject : MonoBehaviour, IInteractable
{
    [SerializeField]
    State state;

    public IState<IContextContainer> State => state;

    public void Interact()
    {
    }

    [DisplayName("Sample/RotateObjectMoveMent")]
    partial class RotateObjectMovement : IBehaviour
    {
        [SerializeField]
        Transform target;
        [SerializeField]
        Transform handle;
        [SerializeField]
        InputActionProperty moveDirection;
        [SerializeField]
        float rotateSpeed = 30f;
        public void OnEnter(IContextContainer obj) { }
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj)
        {
            if (!moveDirection.action.enabled)
                moveDirection.action.Enable();
            var val = moveDirection.action.ReadValue<Vector2>();
            var vec = Camera.main.transform.rotation * new Vector3(val.x, 0, val.y);
            vec.y = 0;
            var dot = Vector3.Dot(vec.normalized, handle.forward);
            if (Mathf.Abs(dot) < 0.5f) return;
            target.Rotate(Vector3.up * rotateSpeed * Mathf.Sign(dot) * Time.fixedDeltaTime);
        }
    }
}

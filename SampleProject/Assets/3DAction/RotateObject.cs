using FriendSea.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateObject : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    float rotateSpeed = 30f;
    [SerializeField]
    InputActionProperty actionButton;
    [SerializeField]
    InputActionProperty moveDirection;
    [SerializeField]
    State state;

    Collider[] results = new Collider[1];
    private void FixedUpdate()
    {
        if (!actionButton.action.enabled)
            actionButton.action.Enable();
        var inArea = Physics.OverlapSphereNonAlloc(transform.position, 0.5f, results) != 0;
        if (!inArea) return;
        var stateMachine = results[0].GetComponent<GameobjectStateMachine>().StateMachine;
        if (actionButton.action.WasPressedThisFrame())
            stateMachine.ForceState(state);
        if (stateMachine.PrimaryLayer.CurrentState == state)
            Rotate();
    }

    void Rotate()
    {
        if (!moveDirection.action.enabled)
            moveDirection.action.Enable();
        var val = moveDirection.action.ReadValue<Vector2>();
        var vec = Camera.main.transform.rotation * new Vector3(val.x, 0, val.y);
        vec.y = 0;
        var dot = Vector3.Dot(vec.normalized, transform.forward);
        if (Mathf.Abs(dot) < 0.5f) return;
        target.Rotate(Vector3.up * rotateSpeed * Mathf.Sign(dot) * Time.fixedDeltaTime);
    }
}

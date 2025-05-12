using FriendSea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField]
    InputActionProperty interactButton;
    [SerializeField]
    LayerMask interactLayerMask;
    [SerializeField]
    GameObject instructionPanel;

    GameobjectStateMachine gameobjectStateMachine;

    Collider[] colliders = new Collider[1];
    private void FixedUpdate()
    {
        if (!interactButton.action.enabled)
            interactButton.action.Enable();
        gameobjectStateMachine ??= GetComponent<GameobjectStateMachine>();
        var inArea = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up, 0f, colliders, interactLayerMask, QueryTriggerInteraction.Collide) != 0;
        instructionPanel.SetActive(inArea && gameobjectStateMachine.IsInDefaultState);
        if (!inArea) return;
        var target = colliders[0].GetComponent<IInteractable>();
        if (interactButton.action.WasPressedThisFrame())
        {
            target.Interact();
            if(target.State != null)
                gameobjectStateMachine.StateMachine.ForceState(target.State);
        }
    }
}

public interface IInteractable
{
    void Interact();
    IState<IContextContainer> State { get; }
}

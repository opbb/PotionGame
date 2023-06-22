using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WizardAnimations : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Walk, 
        Jump
    }

    Animator anim;
    FSMStates currentState;
    CharacterController controller;

    float horizontalInput;
    float verticalInput;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        currentState = FSMStates.Idle;
    }

    private void UpdateIdleState()
    {
        anim.SetInteger("animState", 0);

        if (horizontalInput != 0 || verticalInput != 0)
        {
            currentState = FSMStates.Walk;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            currentState = FSMStates.Jump;
        }
    }

    private void UpdateWalkState()
    {
        anim.SetInteger("animState", 1);

        if (Input.GetKey(KeyCode.Space))
        {
            currentState = FSMStates.Jump;
        }

        if (horizontalInput == 0 && verticalInput == 0)
        {
            currentState = FSMStates.Idle;
        }
    }

    private void UpdateJumpState()
    {
        anim.SetInteger("animState", 2);

        if (controller.isGrounded)
        {
            currentState = FSMStates.Idle;
        }
    }

   
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        switch (currentState)
        {
            case FSMStates.Idle:
                UpdateIdleState();
                break;
            case FSMStates.Walk:
                UpdateWalkState();
                break;
            case FSMStates.Jump:
                UpdateJumpState();
                break;
        }
    }
}


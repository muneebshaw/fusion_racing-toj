using Fusion;
using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ArcadeCarController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 20f;
    public float acceleration = 10f;
    public float reverseSpeed = 10f;
    public float brakeForce = 20f;
    public float turnSpeed = 5f;
    public float gravity = 20f;

    [Header("Drift Settings")]
    public float driftFactor = 0.95f;
    public float traction = 1f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed = 0f;
    private float verticalInput = 0;
    private float turnInput = 0f;

    private bool actionPressed;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        Physics.SyncTransforms(); // auto sync transforms is disabled in the physics settings, so characterController.Move()
                                  // won't necessarily be aware of the new pose as set by the transform unless a FixedUpdate
                                  // or Physics.Simulate() called happened in-between transform.position and CC.Move()
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 300, 20), $"verticalInput: {verticalInput}");
    //    GUI.Label(new Rect(10, 30, 300, 20), $"turnInput: {turnInput}");
    //}

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            actionPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        GetInput();
        ApplyMovement();
        ApplyGravity();
        ApplyDrift();
    }

    private void GetInput()
    {
        // Forward/Backward input (W/S or Up/Down arrows)
        verticalInput = Input.GetAxis("Vertical");

        // Turning input (A/D or Left/Right arrows)
        turnInput = Input.GetAxis("Horizontal");

        // Calculate speed based on input
        if (verticalInput > 0)
        {
            // Accelerate forward
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed * verticalInput, acceleration * Runner.DeltaTime);
        }
        else if (verticalInput < 0)
        {
            // Reverse
            currentSpeed = Mathf.Lerp(currentSpeed, reverseSpeed * verticalInput, acceleration * Runner.DeltaTime);
        }
        else
        {
            // Brake when no input
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, brakeForce * Runner.DeltaTime);
        }

        // Action
        if (actionPressed)
        {
            actionPressed = false;
            Debug.Log("Action pressed!");
            controller.Move(new Vector3(0, 1, 0));
        }
    }

    private void ApplyMovement()
    {
        // Forward movement
        moveDirection = transform.forward * currentSpeed;

        // Turning - only turn when moving
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnRate = turnSpeed * (currentSpeed / maxSpeed);
            transform.Rotate(0f, turnInput * turnRate * Runner.DeltaTime, 0f);
        }

        // Apply movement using CharacterController
        controller.Move(moveDirection * Runner.DeltaTime);
    }

    private void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            controller.Move(gravity * Runner.DeltaTime * Vector3.down);
        }
        else // slight downward force to keep the car grounded
        {
            controller.Move(new Vector3(0, -1, 0));
        }
    }

    private void ApplyDrift()
    {
        // Simple drift effect - car slides a bit when turning
        if (Mathf.Abs(turnInput) > 0.1f && Mathf.Abs(currentSpeed) > 1f)
        {
            moveDirection = Vector3.Lerp(moveDirection.normalized, transform.forward, traction * Runner.DeltaTime) * moveDirection.magnitude;
            moveDirection *= driftFactor;
        }
    }

    internal void SetInputEnabled(bool v)
    {
        enabled = v;
    }
}
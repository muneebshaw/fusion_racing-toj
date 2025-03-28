using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class ArcadeCarControllerNetwork : NetworkBehaviour
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

    private NetworkCharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed = 0f;
    private float verticalInput = 0;
    private float turnInput = 0f;

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"verticalInput: {verticalInput}");
        GUI.Label(new Rect(10, 30, 300, 20), $"turnInput: {turnInput}");
    }

    public override void Spawned()
    {
        controller = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        GetInput();
        ApplyMovement();
        //ApplyGravity();
        ApplyDrift();
    }

    private void GetInput()
    {
        //float verticalInput = 0;

        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            // Forward/Backward input (W/S or Up/Down arrows)
            verticalInput = data.direction.z;

            // Turning input (A/D or Left/Right arrows)
            turnInput = data.direction.x;
        }

        

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

    //private void ApplyGravity()
    //{
    //    if (!controller.Grounded)
    //    {
    //        controller.Move(gravity * Runner.DeltaTime * Vector3.down);
    //    }
    //}

    private void ApplyDrift()
    {
        // Simple drift effect - car slides a bit when turning
        if (Mathf.Abs(turnInput) > 0.1f && Mathf.Abs(currentSpeed) > 1f)
        {
            moveDirection = Vector3.Lerp(moveDirection.normalized, transform.forward, traction * Runner.DeltaTime) * moveDirection.magnitude;
            moveDirection *= driftFactor;
        }
    }
}
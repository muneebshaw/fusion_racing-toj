using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    //public Vector3 direction;
    public bool accelerate;
    public bool decelerate;
    public bool steerLeft;
    public bool steerRight;
    public bool handBrakeOn;
    public bool handBrakeOff;
}
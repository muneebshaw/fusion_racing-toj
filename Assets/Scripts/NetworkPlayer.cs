using Fusion;
using System;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(ReadinessChanged))] public bool IsReady { get; set; }

    private void ReadinessChanged()
    {
        UIManager.Instance.UpdateReadyPlayers();
    }

    [Networked] public NetworkString<_16> PlayerName { get; set; }
    //[Networked] public TickTimer FinishTime { get; set; }

    internal ArcadeCarController carController;

    public override void Spawned()
    {
        carController = GetComponent<ArcadeCarController>();

        if (HasStateAuthority)
        {
            PlayerName = "Player" + Runner.LocalPlayer.PlayerId;
            CameraFollow.instance.SetUp(transform.GetChild(0));

            RaceManager.Instance.RPC_RegisterPlayer(Object.InputAuthority);
            carController.SetInputEnabled(false);
        }

        Debug.Log($"{PlayerName} spawned at {transform.position}");
        UIManager.Instance.HandlePlayerJoined(/*Object.InputAuthority,*/ this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            Debug.Log($"{PlayerName} finished!");
            RaceManager.Instance.RPC_RegisterFinish(Object.InputAuthority);
        }
    }

    private void OnGUI()
    {
        if (HasStateAuthority)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"IsReady: {IsReady}");
            GUI.Label(new Rect(10, 50, 300, 20), $"PlayerName: {PlayerName}");
            GUI.Label(new Rect(10, 90, 300, 20), $"Host: {Runner.IsSharedModeMasterClient}");
        }
    }


    internal void SetReady(bool _isReady)
    {
        IsReady = _isReady;
        RaceManager.Instance.RPC_SetPlayerReady(Object.InputAuthority, _isReady);
    }
}

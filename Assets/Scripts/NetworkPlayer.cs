using Fusion;
using System;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(ReadinessChanged))] public bool IsReady { get; set; }

    private void ReadinessChanged()
    {
        if (RaceManager.IsHost)
        {
            Debug.Log($"ReadinessChanged: {Object.InputAuthority} IsReady:{IsReady}");
            RaceManager.Instance.SetPlayerReady(Object.InputAuthority, IsReady);
        }

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

            //RaceManager.Instance.RegisterPlayer(Object.InputAuthority);
            RPC_RequestRegistration(Object.InputAuthority);
            carController.SetInputEnabled(false);
        }

        Debug.Log($"{PlayerName} spawned at {transform.position}");
        UIManager.Instance.HandlePlayerJoined(/*Object.InputAuthority,*/ this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return; // only allow the local player to trigger the finish line

        if (other.CompareTag("FinishLine"))
        {
            //RaceManager.Instance.RPC_RegisterFinish(Object.InputAuthority);
            RPC_RequestFinish(Object.InputAuthority);
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
        //RaceManager.Instance.RPC_SetPlayerReady(Object.InputAuthority, _isReady);
        Debug.Log($"SetReady: {_isReady}");
        //RPC_RequestReadinessChange(Object.InputAuthority, _isReady);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_RequestRegistration(PlayerRef player)
    {
        // Host receives this RPC and registers the player via RaceManager
        Debug.LogError($"RPC_RequestRegistration: {player} PlayerName:{PlayerName}");
        if (RaceManager.IsHost)
        {
            Debug.LogError($"RPC_RequestRegistration2: {player} PlayerName:{PlayerName}");
            RaceManager.Instance.RegisterPlayer(player);
        }
    }

    //[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    //public void RPC_RequestReadinessChange(PlayerRef player, bool _isReady)
    //{
    //    Debug.LogError($"RPC_RequestReadinessChange: {player} IsReady:{_isReady}");
    //    if (RaceManager.Instance != null)
    //    {
    //        Debug.LogError($"RPC_RequestReadinessChange2: {player} IsReady:{_isReady}");
    //        RaceManager.Instance.RPC_SetPlayerReady(player, _isReady);
    //    }
    //}

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_RequestFinish(PlayerRef player)
    {
        Debug.LogError($"RPC_RequestFinish: {PlayerName}");
        if (RaceManager.IsHost)
        {
            RaceManager.Instance.RegisterFinish(player);
        }
    }

}

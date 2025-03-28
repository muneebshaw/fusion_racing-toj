using Fusion;
using System.Linq;
using UnityEngine;

public enum RaceState { Lobby, Countdown, Racing, Finished }

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Networked] public RaceState CurrentState { get; set; }
    [Networked] public float CountdownTimer { get; set; }
    [Networked] public float RaceTimer { get; set; }

    [Networked, Capacity(4)]
    public NetworkDictionary<PlayerRef, float> FinishTimes { get; }

    [Networked, Capacity(4)]
    public NetworkDictionary<PlayerRef, bool> ReadyPlayers { get; }

    [SerializeField] private float _countdownDuration = 5f;
    [SerializeField] internal float _raceDuration = 120f;

    private Transform _finishLine;

    internal bool IsSpawned = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void Spawned()
    {
        IsSpawned = true;
        _finishLine = GameObject.FindGameObjectWithTag("FinishLine").transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsSharedModeMasterClient) return;

        switch (CurrentState)
        {
            case RaceState.Lobby:
            //    CheckAllReady();
            //    break;
            case RaceState.Countdown:
                //    CountdownTimer -= Runner.DeltaTime;
                //    if (CountdownTimer <= 0)
                //    {
                //        CurrentState = RaceState.Racing;
                //        RaceEvents.OnRaceStart?.Invoke();
                //        RPC_ReleaseAllCars();
                //        RaceTimer = _raceDuration;
                //    }
                if (ReadyPlayers.Count == Runner.ActivePlayers.Count() &&
                ReadyPlayers.All(p => p.Value))
                {
                    CurrentState = RaceState.Countdown;
                    CountdownTimer -= Runner.DeltaTime;
                    if (CountdownTimer <= 0)
                    {
                        CurrentState = RaceState.Racing;
                        RaceEvents.OnRaceStart?.Invoke();
                        RPC_ReleaseAllCars();
                        RaceTimer = _raceDuration;
                    }
                }
                else
                {
                    CountdownTimer = _countdownDuration;
                    CurrentState = RaceState.Lobby;
                }
                break;
            case RaceState.Racing:
                RaceTimer -= Runner.DeltaTime;
                CheckRaceEnd();
                break;
        }
    }

    private void OnGUI()
    {
        if (Runner.IsSharedModeMasterClient)
        {
            GUI.Label(new Rect(400, 10, 300, 20), $"CurrentState: {CurrentState}");
            GUI.Label(new Rect(400, 50, 300, 20), $"ReadyPlayers: {ReadyPlayers.Count}");
            GUI.Label(new Rect(400, 90, 300, 20), $"Runner.ActivePlayers: {Runner.ActivePlayers.Count()}");
        }
    }

    //private void CheckAllReady()
    //{
    //    if (ReadyPlayers.Count == Runner.ActivePlayers.Count() &&
    //        ReadyPlayers.All(p => p.Value))
    //    {
    //        CurrentState = RaceState.Countdown;
    //        CountdownTimer = _countdownDuration;
    //    }
    //}

    private void CheckRaceEnd()
    {
        bool allFinished = ReadyPlayers.All(p => FinishTimes.Get(p.Key)> 0);
        bool timeExpired = RaceTimer <= 0;

        if (allFinished || timeExpired)
        {
            CurrentState = RaceState.Finished;
            RaceEvents.OnRaceEnd?.Invoke();
            RPC_StopAllCars();
        }
    }


    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RegisterPlayer(PlayerRef player)
    {
        Debug.Log($"RPC_RegisterPlayer {player.PlayerId}");
        FinishTimes.Set(player, 0);
        ReadyPlayers.Set(player, false);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SetPlayerReady(PlayerRef player, bool isReady)
    {
        Debug.LogError($"RPC_SetPlayerReady");
        ReadyPlayers.Set(player, isReady);
        Debug.Log($"RPC_SetPlayerReady: {player} IsReady:{ReadyPlayers.Get(player)}");
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RegisterFinish(PlayerRef player)
    {
        FinishTimes.Set(player, _raceDuration - RaceTimer);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_ReleaseAllCars()
    {
        foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
        {
            player.carController.SetInputEnabled(true);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_StopAllCars()
    {
        foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
        {
            player.carController.SetInputEnabled(false);
        }
    }

    public float GetDistanceToFinish(Vector3 position)
    {
        return Vector3.Distance(position, _finishLine.position);
    }
}
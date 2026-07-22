using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Linq;
using UnityEngine;

public enum RaceState { Lobby, Countdown, Racing, Finished }

public class RaceManager : NetworkBehaviour
{

    [Networked] public RaceState CurrentState { get; set; }
    [Networked] public float CountdownTimer { get; set; }
    [Networked] public float RaceTimer { get; set; }

    [Networked, Capacity(4)]
    public NetworkDictionary<PlayerRef, float> FinishTimes { get; }

    [Networked, Capacity(4)]
    public NetworkLinkedList<NetworkPlayer> Progresses { get; }

    [Networked, Capacity(4)]
    public NetworkDictionary<PlayerRef, bool> ReadyPlayers { get; }

    [SerializeField] private float _countdownDuration = 5f;
    [SerializeField] internal float _raceDuration = 120f;

    private Transform _finishLine;

    internal bool IsSpawned = false;

    public static RaceManager Instance { get; private set; }

    internal static bool IsHost;

    public override void Spawned()
    {
        IsHost = Runner.IsSharedModeMasterClient;

        //if (Runner.IsSharedModeMasterClient)
        {
            Instance = this;
        }

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

    //private void OnGUI()
    //{
    //    if (Runner && Runner.IsSharedModeMasterClient)
    //    {
    //        var readyPlayers = ReadyPlayers.Where(p => p.Value).Select(p => p.Key.PlayerId).ToList();

    //        GUI.Label(new Rect(400, 10, 300, 20), $"CurrentState: {CurrentState}");
    //        GUI.Label(new Rect(400, 50, 300, 20), $"ReadyPlayers: {readyPlayers.Count}");
    //        GUI.Label(new Rect(400, 90, 300, 20), $"Runner.ActivePlayers: {Runner.ActivePlayers.Count()}");
    //        GUI.Label(new Rect(400, 130, 300, 20), $"FinishTimes: {FinishTimes.Count}");
    //    }
    //}

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
        //allFinished = ReadyPlayers.All(p => FinishTimes.Get(p.Key)> 0);

        bool allFinished = true;
        foreach (var player in ReadyPlayers)
        {
            if (FinishTimes.TryGet(player.Key, out float time))
            {
                if (time <= 0)
                {
                    allFinished = false;
                    break;
                }
            }
            else
            {
                allFinished = false;
                break;
            }
        }

        //bool allFinished = FinishTimes.Count == Runner.ActivePlayers.Count() && FinishTimes.All(p => p.Value > 0);


        bool timeExpired = RaceTimer <= 0;

        if (allFinished || timeExpired)
        {
            CurrentState = RaceState.Finished;
            RaceEvents.OnRaceEnd?.Invoke();
            RPC_StopAllCars();
        }
    }


    internal void RegisterPlayer(PlayerRef player, NetworkPlayer networkPlayer)
    {
        Debug.Log($"RPC_RegisterPlayer {player.PlayerId}");
        FinishTimes.Set(player, 0);
        if (!Progresses.Contains(networkPlayer))
        {
            Progresses.Add(networkPlayer);
        }
        ReadyPlayers.Set(player, false);
    }

    internal void SetPlayerReady(PlayerRef player, bool isReady)
    {
        ReadyPlayers.Set(player, isReady);
        Debug.Log($"SetPlayerReady: {player} IsReady:{ReadyPlayers.Get(player)}");
    }

    //[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    internal void RegisterFinish(PlayerRef player)
    {
        Debug.LogError($"RegisterFinish {player.PlayerId}");
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
        Debug.LogError($"RPC_StopAllCars");
        foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
        {
            player.carController.SetInputEnabled(false);
        }
    }

    //public float GetDistanceToFinish(Vector3 position)
    //{
    //    return Vector3.Distance(position, _finishLine.position);
    //}
}
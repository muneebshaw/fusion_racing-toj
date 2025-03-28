using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    internal static UIManager Instance;

    [Header("Lobby UI")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private Transform _readyPlayersParent;
    [SerializeField] private ReadyPlayerEntry _readyPlayerEntry;

    [Header("In-Game UI")]
    [SerializeField] private GameObject _racePanel;
    [SerializeField] private TextMeshProUGUI _raceTimer;

    [Header("Finish UI")]
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private LeaderboardEntry _entryPrefab;
    [SerializeField] private Transform _leaderboardContent;

    private NetworkPlayer _localNetworkPlayer;
    //private PlayerRef _localPlayerRef;
    private readonly List<LeaderboardEntry> _entries = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        RaceEvents.OnRaceStart += RaceStarted;
    }

    private void OnDisable()
    {
        RaceEvents.OnRaceStart -= RaceStarted;
    }

    private void RaceStarted()
    {
        //
    }

    private void Update()
    {
        if (RaceManager.Instance == null || !RaceManager.Instance.IsSpawned) return;

        UpdateUIState();
        UpdateInGameUI();
        UpdateLeaderboard();
    }

    private void UpdateUIState()
    {
        _lobbyPanel.SetActive(RaceManager.Instance.CurrentState is RaceState.Lobby or RaceState.Countdown);
        _racePanel.SetActive(RaceManager.Instance.CurrentState == RaceState.Racing);
        _finishPanel.SetActive(RaceManager.Instance.CurrentState == RaceState.Finished);
    }

    private void UpdateInGameUI()
    {
        _countdownText.gameObject.SetActive(RaceManager.Instance.CurrentState == RaceState.Countdown);

        if (RaceManager.Instance.CurrentState == RaceState.Countdown)
        {
            _countdownText.text = Mathf.CeilToInt(RaceManager.Instance.CountdownTimer).ToString();
        }
        else if (RaceManager.Instance.CurrentState == RaceState.Racing)
        {
            _raceTimer.text = RaceManager.Instance.RaceTimer.ToString("F1");
        }
    }

    private void UpdateLeaderboard()
    {
        var players = GetSortedPlayers();

        // Pool entries
        while (_entries.Count < players.Count)
        {
            _entries.Add(Instantiate(_entryPrefab, _leaderboardContent));
        }

        for (int i = 0; i < players.Count; i++)
        {
            var entry = _entries[i];
            bool isLocal = players[i] == _localNetworkPlayer.Object.InputAuthority;

            entry.Setup(
                position: i + 1,
                playerName: $"Player {players[i].PlayerId}",
                time: GetPlayerTime(players[i]),
                isLocal: isLocal
            );
        }
    }

    private List<PlayerRef> GetSortedPlayers()
    {
        if (RaceManager.Instance == null || RaceManager.Instance.Runner == null)
            return new List<PlayerRef>();

        // Create a dictionary to map PlayerRefs to their NetworkPlayer instances
        var playerMap = new Dictionary<PlayerRef, NetworkPlayer>();
        foreach (var np in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
        {
            playerMap[np.Object.InputAuthority] = np;
        }

        return RaceManager.Instance.Runner.ActivePlayers
            .OrderByDescending(p => RaceManager.Instance.FinishTimes.ContainsKey(p)) // Finished players first
            .ThenBy(p =>
            {
                // For finished players: sort by finish time
                if (RaceManager.Instance.FinishTimes.TryGet(p, out float time))
                    return time;

                // For racing players: sort by distance to finish line
                if (playerMap.TryGetValue(p, out NetworkPlayer np))
                    return RaceManager.Instance.GetDistanceToFinish(np.transform.position);

                return float.MaxValue; // Fallback for missing players
            })
            .ToList();
    }

    private string GetPlayerTime(PlayerRef player)
    {
        if (RaceManager.Instance.FinishTimes.TryGet(player, out float time))
        {
            return time.ToString("F1");
        }
        return (RaceManager.Instance._raceDuration - RaceManager.Instance.RaceTimer).ToString("F1");
    }

    private List<ReadyPlayerEntry> readyPlayerEntries = new();
    internal void HandlePlayerJoined(NetworkPlayer netPlayer = null)
    {
        if (netPlayer.HasStateAuthority)
        { 
            _localNetworkPlayer = netPlayer;
            //_localPlayerRef = playerRef;
        }

        var entry = Instantiate(_readyPlayerEntry, _readyPlayersParent);
        readyPlayerEntries.Add(entry);

        //bool isLocal = netPlayer != null;
        //bool isReady = RaceManager.Instance.ReadyPlayers.TryGet(playerRef, out bool ready) && ready;
        entry.Setup(/*_localNetworkPlayer.PlayerName.ToString(), isLocal, */netPlayer);
    }

    internal void UpdateReadyPlayers()
    {
        Debug.Log("UpdateReadyPlayers");
        foreach (var entry in readyPlayerEntries)
        {
            //bool isReady = RaceManager.Instance.ReadyPlayers.Get(_localPlayerRef);
            entry.UpdateReadyStatus();
        }
    }
}

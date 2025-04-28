using Fusion;
using TMPro;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(ReadinessChanged))] public bool IsReady { get; set; }

    [SerializeField] private TextMeshProUGUI progressText;

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
    [Networked, OnChangedRender(nameof(TrackProgressChanged))] public float TrackProgress { get; set; }
    private void TrackProgressChanged()
    {
        progressText.text = "Progress: " + (TrackProgress * 100).ToString("F0") + "%";
    }
    //[Networked] public TickTimer FinishTime { get; set; }

    internal ArcadeCarController carController;
    private SplineFollower splineFollower;

    public override void Spawned()
    {
        carController = GetComponent<ArcadeCarController>();
        splineFollower = GetComponent<SplineFollower>();

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

    private void Update()
    {
        if (HasStateAuthority)
        {
            TrackProgress = splineFollower.GetDistanceAlongSpline();
            //Debug.Log($"TrackProgress: {TrackProgress}");
        }
    }


    internal void SetReady(bool _isReady)
    {
        IsReady = _isReady;
        Debug.Log($"SetReady: {_isReady}");
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_RequestRegistration(PlayerRef player)
    {
        // Host receives this RPC and registers the player via RaceManager
        if (RaceManager.IsHost)
        {
            RaceManager.Instance.RegisterPlayer(player);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_RequestFinish(PlayerRef player)
    {
        Debug.Log($"RPC_RequestFinish: {PlayerName}");
        if (RaceManager.IsHost)
        {
            RaceManager.Instance.RegisterFinish(player);
        }
    }
}

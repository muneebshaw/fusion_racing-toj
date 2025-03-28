using Fusion;
using UnityEngine;

public class SharedSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerCarPrefab;
    [SerializeField] private Transform spawnPoint;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Vector3 spawnPosition = spawnPoint.position + new Vector3((player.RawEncoded % Runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            Runner.Spawn(playerCarPrefab, spawnPosition, Quaternion.identity, Runner.LocalPlayer);
        }
    }
}

using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ShareSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] List<GameObject> VehiclePrefabs;
    [SerializeField] List<GameObject> DriverPrefabs;

    [SerializeField] GameObject playerPrefab1;
    [SerializeField] GameObject playerPrefab2;
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (Runner.SessionInfo.PlayerCount == 1)
            {
                var prefab = VehiclePrefabs[0];
                Runner.Spawn(prefab, new Vector3(-7, 7, -10), Quaternion.identity);
            }
            else
            {
                var prefab = VehiclePrefabs[1];
                Runner.Spawn(prefab, new Vector3(-7, 7, -9), Quaternion.identity);
            }
        }
    }
}

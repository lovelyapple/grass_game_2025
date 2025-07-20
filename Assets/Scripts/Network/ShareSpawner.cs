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
            GameObject vehiclePrefab = null;
            Vector3 spawnPosition = Vector3.zero;
            if (Runner.SessionInfo.PlayerCount == 1)
            {
                vehiclePrefab = VehiclePrefabs[0];
                spawnPosition = new Vector3(-7, 7, -10);
            }
            else
            {
                vehiclePrefab = VehiclePrefabs[1];
                spawnPosition = new Vector3(-7, 7, -8);
            }

            var vh = Runner.Spawn(vehiclePrefab, spawnPosition, Quaternion.identity).GetComponent<VehicleBase>();
            vh.Registry(GameInputController.Instance);
        }
    }
}

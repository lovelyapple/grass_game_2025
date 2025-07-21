using System.Collections.Generic;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class ShareSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] List<GameObject> VehiclePrefabs;
    [SerializeField] List<GameObject> DriverPrefabs;
    [SerializeField] GameObject RpcConnectorPrefab;
    [SerializeField] GameObject PlayerInfoObjectPrefab;

    [SerializeField] GameObject RoomStateControllerPrefab;
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (GameCoreModel.Instance.IsAdminUser)
            {
                var roomStateController = Runner.Spawn(RoomStateControllerPrefab, Vector3.zero, Quaternion.identity).GetComponent<RoomStateController>();
                roomStateController.SetupId(player.PlayerId);
                Runner.Spawn(RpcConnectorPrefab, Vector3.zero, Quaternion.identity);
            }
            else
            {
                var playerObject = Runner.Spawn(PlayerInfoObjectPrefab, Vector3.zero, Quaternion.identity, inputAuthority: player).GetComponent<PlayerInfoObject>();
                playerObject.Initialize(GameCoreModel.Instance.SelfName, player);
                GamePlayerInfoModel.GetInstance().SetSelfObject(playerObject);


                // GameObject vehiclePrefab = null;
                // Vector3 spawnPosition = Vector3.zero;
                // if (Runner.SessionInfo.PlayerCount == 1)
                // {
                //     vehiclePrefab = VehiclePrefabs[0];
                //     spawnPosition = new Vector3(-7, 7, -10);
                // }
                // else
                // {
                //     vehiclePrefab = VehiclePrefabs[1];
                //     spawnPosition = new Vector3(-7, 7, -8);
                // }

                // var vh = Runner.Spawn(vehiclePrefab, spawnPosition, Quaternion.identity).GetComponent<VehicleBase>();
                // vh.Registry(GameInputController.Instance);
            }
        }
    }
}

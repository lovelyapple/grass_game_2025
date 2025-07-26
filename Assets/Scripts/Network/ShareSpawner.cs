using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class ShareSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] List<GameObject> VehiclePrefabs;
    [SerializeField] List<GameObject> DriverPrefabs;
    [SerializeField] List<GameObject> SaddlePrefabs;

    [SerializeField] GameObject RpcConnectorPrefab;
    [SerializeField] GameObject PlayerInfoObjectPrefab;

    [SerializeField] GameObject RoomStateControllerPrefab;
    public static ShareSpawner Instance{ get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (GameCoreModel.Instance.IsAdminUser)
            {
                Runner.Spawn(RoomStateControllerPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<RoomStateController>();
                Runner.Spawn(RpcConnectorPrefab, Vector3.zero, Quaternion.identity);
                
                ModelCache.Admin.OnAdminJoined(player);
            }
            else
            {
                Debug.Log($"{player.PlayerId}");
                var playerObject = Runner.Spawn(PlayerInfoObjectPrefab, Vector3.zero, Quaternion.identity, inputAuthority: player)
                    .GetComponent<PlayerInfoObject>();

                playerObject.InitializeAsync(GamePlayerInfoModel.GetInstance().SelfName, player).Forget();
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
    public FieldPlayerController LoadVehicle(Vehicles vehicle)
    {
        var prefab = VehiclePrefabs.FirstOrDefault(x => x.name.Contains(vehicle.ToString()));
        var ctrl = Runner.Spawn(prefab, Vector3.zero, Quaternion.identity).GetComponent<FieldPlayerController>();
        return ctrl;
    }

    // public VehicleBase LoadVehicle(Vehicles vehicle)
    // {

    // }
    // public 
}

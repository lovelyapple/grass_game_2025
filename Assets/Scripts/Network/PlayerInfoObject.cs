using Fusion;
using UnityEngine;

public class PlayerInfoObject : NetworkBehaviour
{
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public PlayerEquipmentSetInfoStruct PlayerEquipment { get; set; }
    public void Initialize(string playerName, PlayerRef playerRef)
    {
        PlayerName = playerName;
        PlayerRef = playerRef;
        PlayerEquipment = new EquipmentSetInfo().ToStruct();
    }
    public void UpdateEquipment(EquipmentSetInfo setInfo)
    {
        PlayerEquipment = setInfo.ToStruct();
    }
    public override void Spawned()
    {
        PlayerRootObject.Instance.OnPlayerInfoSpawned(this);
    }
}

using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class MatchPlayerModel
{
    public int PlayerId { get; private set; }
    public FieldPlayerController FieldPlayerController { get; private set; }
    private EquipmentSetInfo _equipmentInfo;
    public bool IsResourceReady => FieldPlayerController != null && FieldPlayerController.IsReady;
    public int PlayerIndex{ get; private set; }
    public void Initialize(PlayerInfoObject obj)
    {
        PlayerId = obj.PlayerId;
        _equipmentInfo = new EquipmentSetInfo(obj.PlayerEquipment);
    }
    public void SetupIndex(int index)
    {
        PlayerIndex = index;
    }
    public void LoadCtrlPlayerSelf()
    {
        if (PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            ShareSpawner.Instance.LoadVehicle(_equipmentInfo.Vehicle);
        }
    }
    public void OnFieldPlayerControllerSpawned(FieldPlayerController controller)
    {
        FieldPlayerController = controller;
    }
}

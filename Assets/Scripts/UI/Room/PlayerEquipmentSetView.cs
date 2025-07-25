using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum Characters
{
    JK,
    OfficeWorker,
    Sumo,
    Max,
}
public enum SaddleType
{
    Ice,
    Leather,
    Iron,
    Max,
}
public enum Vehicles
{
    Bicycle,
    Scooter,
    Unicycle,
    Max,
}
[SerializeField]
public class EquipmentSetInfo
{
    public int PlayerId;
    public Characters Character;
    public SaddleType Saddle;
    public Vehicles Vehicle;
    public EquipmentSetInfo()
    {

    }
    public EquipmentSetInfo Clone()
    {
        return new EquipmentSetInfo()
        {
            PlayerId = PlayerId,
            Character = Character,
            Saddle = Saddle,
            Vehicle = Vehicle,
        };
    }

    public EquipmentSetInfo(PlayerEquipmentSetInfoStruct set)
    {
        PlayerId = set.PlayerId;
        Character = (Characters)set.Character;
        Saddle = (SaddleType)set.SaddleType;
        Vehicle = (Vehicles)set.Vehicle;
    }

    public PlayerEquipmentSetInfoStruct ToStruct()
    {
        var result = new PlayerEquipmentSetInfoStruct();
        result.PlayerId = PlayerId;
        result.Character = (int)Character;
        result.SaddleType = (int)Saddle;
        result.Vehicle = (int)Vehicle;
        return result;
    }
}
public class PlayerEquipmentSetView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] Image CharacterImage;
    [SerializeField] Image SaddleImage;
    [SerializeField] Image VehicleImage;
    public int PlayerId { get; private set; }
    private CancellationTokenSource _requestNameHandler;
    public void UpdateEquipmentInfo(EquipmentSetInfo equipmentSetInfo)
    {
        PlayerId = equipmentSetInfo.PlayerId;
        CharacterImage.sprite = ResourceContainer.Instance.GetCharacterImage(equipmentSetInfo.Character, false);
        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(equipmentSetInfo.Saddle, false);
        VehicleImage.sprite = ResourceContainer.Instance.GetVehicleImage(equipmentSetInfo.Vehicle, false);
        gameObject.SetActive(true);

        RequestPlayerNameAsync().Forget();
    }
    public void InitAsSelf(int playerId)
    {
        NameText.text = "Joinnig";
        PlayerId = playerId;
        CharacterImage.sprite = ResourceContainer.Instance.GetCharacterImage(Characters.JK, false);
        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(SaddleType.Ice, false);
        VehicleImage.sprite = ResourceContainer.Instance.GetVehicleImage(Vehicles.Bicycle, false);
        gameObject.SetActive(true);
        RequestPlayerNameAsync().Forget();
    }
    private async UniTask<Unit> RequestPlayerNameAsync()
    {
        _requestNameHandler = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(_requestNameHandler.Token, destroyCancellationToken).Token;
        await UniTask.WaitUntil(() => PlayerRootObject.Instance.GetPlayerInfoName(PlayerId) != null, cancellationToken: token);
        NameText.text = PlayerRootObject.Instance.GetPlayerInfoName(PlayerId);
        return Unit.Default;
    }
    public void SetAsEmpty()
    {
        PlayerId = 0;
        gameObject.SetActive(false);

        _requestNameHandler?.Cancel();
        NameText.text = "";
    }
}

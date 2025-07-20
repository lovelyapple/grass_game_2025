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
    public EquipmentSetInfo(EquipmentSetInfoStruct set)
    {
        PlayerId = set.PlayerId;
        Character = (Characters)set.Character;
        Saddle = (SaddleType)set.SaddleType;
        Vehicle = (Vehicles)set.Vehicle;
    }
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
}
public class PlayerEquipmentSetView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] Image CharacterImage;
    [SerializeField] Image SaddleImage;
    [SerializeField] Image VehicleImage;
    public int PlayerId { get; private set; }
    public void UpdateEquipmentInfo(string playerName, EquipmentSetInfo equipmentSetInfo)
    {
        NameText.text = playerName;
        PlayerId = equipmentSetInfo.PlayerId;
        CharacterImage.sprite = ResourceContainer.Instance.GetCharacterImage(equipmentSetInfo.Character, false);
        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(equipmentSetInfo.Saddle, false);
        VehicleImage.sprite = ResourceContainer.Instance.GetVehicleImage(equipmentSetInfo.Vehicle, false);
        gameObject.SetActive(true);
    }
    public void InitAsSelf(int playerId)
    {
        NameText.text = "self";
        PlayerId = playerId;
        CharacterImage.sprite = ResourceContainer.Instance.GetCharacterImage(Characters.JK, false);
        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(SaddleType.Ice, false);
        VehicleImage.sprite = ResourceContainer.Instance.GetVehicleImage(Vehicles.Bicycle, false);
        gameObject.SetActive(true);

    }
    public void SetAsEmpty()
    {
        PlayerId = 0;
        gameObject.SetActive(false);
    }
}

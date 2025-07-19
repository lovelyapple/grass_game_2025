using UnityEngine;
using UnityEngine.UI;
public enum Characters
{
    JK,
    OfficeWorker,
    Sumo,
}
public enum SaddleType
{
    Ice,
    Leather,
    Iron,
}
public enum Vehicles
{
    Bicycle,
    Scooter,
    Unicycle,
}
[SerializeField]
public class EquipmentSetInfo
{
    public Characters Character;
    public SaddleType Saddle;
    public Vehicles Vehicle;
}
public class PlayerEquipmentSetView : MonoBehaviour
{
    [SerializeField] Image CharacterImage;
    [SerializeField] Image SaddleImage;
    [SerializeField] Image VehicleImage;

    [SerializeField] Button RandomButton;

    private EquipmentSetInfo _equipmentSetInfo;
    private string NemaLabel;
}

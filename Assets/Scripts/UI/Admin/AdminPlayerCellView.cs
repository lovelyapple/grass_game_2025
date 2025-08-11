using System.Linq;
using TMPro;
using UnityEngine;

public class AdminPlayerCellView : MonoBehaviour
{
    private int _playerId;
    [SerializeField] TextMeshProUGUI PlayerId;
    [SerializeField] TextMeshProUGUI PlayerName;
    [SerializeField] TextMeshProUGUI Vehcile;
    [SerializeField] TextMeshProUGUI Saddle;
    [SerializeField] TextMeshProUGUI Chara;
    [SerializeField] GameObject PlayerRef;
    [SerializeField] GameObject PlayerInfoObject;
    [SerializeField] GameObject MatchPlayerodel;
    [SerializeField] GameObject FeildController;
    public void Setup(int playerId)
    {
        _playerId = playerId;
        PlayerId.text = playerId.ToString();
        var playerRefInfo = RoomModel.GetInstance().PlayerInfos.First(x => x.PlayerId == playerId);

        if(playerRefInfo == null)
        {
            PlayerName.text = "";
            Vehcile.text = "";
            Saddle.text = "";
            Chara.text = "";
            PlayerRef.gameObject.SetActive(false);
            PlayerInfoObject.gameObject.SetActive(false);
            MatchPlayerodel.gameObject.SetActive(false);
            FeildController.gameObject.SetActive(false);

            return;
        }

        PlayerRef.gameObject.SetActive(true);

        PlayerRootObject.Instance.PlayerInfos.TryGetValue(playerId, out var infoObject);

        if(infoObject == null)
        {
            Vehcile.text = "";
            Saddle.text = "";
            Chara.text = "";
            PlayerInfoObject.gameObject.SetActive(false);
            MatchPlayerodel.gameObject.SetActive(false);
            FeildController.gameObject.SetActive(false);
            return;
        }

        PlayerInfoObject.gameObject.SetActive(true);
        PlayerName.text = infoObject.PlayerName;
        Vehcile.text = infoObject.PlayerEquipment.Vehicle.ToString();
        Saddle.text = infoObject.PlayerEquipment.SaddleType.ToString();
        Chara.text = infoObject.PlayerEquipment.Character.ToString();

        var playerModel = MatchModel.GetInstance().Players.Find(x => x.PlayerId == playerId);

        if(playerModel == null)
        {
            MatchPlayerodel.gameObject.SetActive(false);
            FeildController.gameObject.SetActive(false);
            return;
        }

        MatchPlayerodel.gameObject.SetActive(true);
        FeildController.gameObject.SetActive(playerModel.FieldPlayerController != null);

    }
}

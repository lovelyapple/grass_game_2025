using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCell : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RoomNameLabel;
    [SerializeField] TextMeshProUGUI PlayerCountLabel;
    [SerializeField] TextMeshProUGUI RoomPhaseLabel;
    [SerializeField] Button JoinRoomBtn;
    public string RoomName = "";
    private int _playerCnt;
    private string _roomPhase;
    public Observable<string> OnClickObservable => JoinRoomBtn
    .OnClickAsObservable()
    .Select(_ => RoomName); 

    public void Initialize(string name, int playerCnt, string roomPhase)
    {
        RoomName = name;
        _playerCnt = playerCnt;
        _roomPhase = roomPhase;
        RoomNameLabel.text = name;
        UpdateCell();

    }
    public void UpdateCell(int playerCnt, string roomPhase)
    {
        _playerCnt = playerCnt;
        _roomPhase = roomPhase;
        UpdateCell();
    }
    private void UpdateCell()
    {
        PlayerCountLabel.text = $"{_playerCnt} / {GameConstant.MaxPlayerPerRoom}";
        RoomPhaseLabel.text = _roomPhase.ToString();
        JoinRoomBtn.interactable = _playerCnt < GameConstant.MaxPlayerPerRoom && _roomPhase == RoomPhase.Waiting.ToString();
    }
}

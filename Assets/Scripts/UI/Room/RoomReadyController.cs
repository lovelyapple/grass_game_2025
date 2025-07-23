using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using StarMessage.Models;

public class RoomReadyController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RoomNameLabel;
    [SerializeField] TextMeshProUGUI PlayerCountLabel;
    [SerializeField] TextMeshProUGUI RoomStateLabel;
    [SerializeField] TextMeshProUGUI CountDownlabel;
    [SerializeField] private PlayerEquipmentSetView SelfEquipmentSetView;
    [SerializeField] private List<PlayerEquipmentSetView> PlayerEquipmentSetViews;
    [SerializeField] Button RandomButton;
    [SerializeField] Button CharaChangeButton;
    [SerializeField] Button SaddleChangeButton;
    [SerializeField] Button VehicleChangeButton;
    [SerializeField] Button ConfirmButton;
    private List<Button> ActionButtonList;
    private void Awake()
    {
        ActionButtonList = new List<Button>()
        {
            RandomButton, CharaChangeButton, SaddleChangeButton, VehicleChangeButton, ConfirmButton,
        };

        PlayerEquipmentModel.GetInstance()
        .PlayerEquipmentUpdateObservable()
        .Subscribe(x => UpdateEquipmentInfo(x))
        .AddTo(this);

        PlayerEquipmentModel.GetInstance()
        .PlayerLeaveObservable()
        .Subscribe(x => OnPlayerLeave(x))
        .AddTo(this);

        CharaChangeButton.OnClickAsObservable()
        .Subscribe(_ => OnClickCharaChange())
        .AddTo(this);

        SaddleChangeButton.OnClickAsObservable()
        .Subscribe(_ => OnClickSaddleChange())
        .AddTo(this);

        VehicleChangeButton.OnClickAsObservable()
        .Subscribe(_ => OnClickVehicleChange())
        .AddTo(this);
        
        ConfirmButton.OnClickAsObservable()
        .Subscribe(_ => OnConfirm())
        .AddTo(this);

        RandomButton.OnClickAsObservable()
        .Subscribe(_ => ShaffleSelfEquipment())
        .AddTo(this);

        SelfEquipmentSetView.InitAsSelf(RoomModel.GetInstance().SelfPlayerRef.PlayerId);
        PlayerEquipmentSetViews.ForEach(x => x.SetAsEmpty());

        var equipmentCache = PlayerRootObject.Instance.PlayerInfos.Values
        .Where(x => x.PlayerId != RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        .Select(x => x.PlayerEquipment);

        foreach(var equipment in equipmentCache)
        {
            UpdateEquipmentInfo(new (equipment));
        }

        RoomNameLabel.text = RoomModel.GetInstance().RoomName;

        RoomModel.GetInstance().OnRoomCountDownStartObservable()
        .Subscribe(_ => OnCountDownStart())
        .AddTo(this);

        RoomModel.GetInstance().OnCountDownUpdatedAsObservable()
        .Subscribe(sec => OnCountDownChange(sec))
        .AddTo(this);

        RoomModel.GetInstance().OnCountDownCancelledAsObservable()
        .Subscribe(sec => OnCountDownCancelled())
        .AddTo(this);

        CountDownlabel.text = "--";

        ActionButtonList.ForEach(x => x.interactable = true);
    }
    private void UpdateEquipmentInfo(EquipmentSetInfo equipmentSetInfo)
    {
        if(equipmentSetInfo.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            SelfEquipmentSetView.UpdateEquipmentInfo(equipmentSetInfo);
        }
        else
        {
            var view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == equipmentSetInfo.PlayerId);

            if(view == null)
            {
                view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == 0);
            }

            view.UpdateEquipmentInfo(equipmentSetInfo);
        }
        UpdatePlayerCountLabel();
    }
    private void OnPlayerLeave(int id)
    {
        var view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == id);
        view.SetAsEmpty();
        UpdatePlayerCountLabel();
    }
    private void OnClickCharaChange()
    {
        PlayerEquipmentModel.GetInstance().RequestChangeSelfChara();
    }
    private void OnClickSaddleChange()
    {
        PlayerEquipmentModel.GetInstance().RequestChangeSelfSaddle();
    }
    private void OnClickVehicleChange()
    {
        PlayerEquipmentModel.GetInstance().RequestChangeSelfVehicle();
    }
    private void OnConfirm()
    {
        PlayerEquipmentModel.GetInstance().SaveSelfEquipment();
    }
    private void ShaffleSelfEquipment()
    {
        PlayerEquipmentModel.GetInstance().ShuffleSelfEquipment();
    }
    private void UpdatePlayerCountLabel()
    {
        var infos = PlayerEquipmentModel.GetInstance().PlayerEquipmentSetInfos.Where(x => x.PlayerId != 1);
        PlayerCountLabel.text = $"{infos.Count()} / {GameConstant.MaxPlayerPerRoom}";
    }
    private void OnCountDownStart()
    {
        
    }
    private void OnCountDownChange(double remainSeconds)
    {
        if(remainSeconds > GameConstant.FinalCountDowneSec)
        {
            CountDownlabel.text = $"{remainSeconds}";
            ActionButtonList.ForEach(x => x.interactable = true);
        }
        else
        {
            CountDownlabel.text = $"{remainSeconds}";
            ActionButtonList.ForEach(x => x.interactable = false);
        }
    }
    private void OnCountDownCancelled()
    {
        ActionButtonList.ForEach(x => x.interactable = true);
        CountDownlabel.text = "--";
    }
}

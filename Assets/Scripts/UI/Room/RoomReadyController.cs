using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using System.Linq;
using UnityEngine.UI;

public class RoomReadyController : MonoBehaviour
{
    [SerializeField] private PlayerEquipmentSetView SelfEquipmentSetView;
    [SerializeField] private List<PlayerEquipmentSetView> PlayerEquipmentSetViews;
    [SerializeField] Button RandomButton;
    [SerializeField] Button CharaChangeButton;
    [SerializeField] Button SaddleChangeButton;
    [SerializeField] Button VehicleChangeButton;
    [SerializeField] Button ConfirmButton;
    private void Awake()
    {
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

        SelfEquipmentSetView.InitAsSelf(RoomModel.GetInstance().SelfPlayerRef.PlayerId);
        PlayerEquipmentSetViews.ForEach(x => x.SetAsEmpty());
    }
    private void UpdateEquipmentInfo(EquipmentSetInfo equipmentSetInfo)
    {
        if(equipmentSetInfo.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            SelfEquipmentSetView.UpdateEquipmentInfo("self", equipmentSetInfo);
        }
        else
        {
            var view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == equipmentSetInfo.PlayerId);

            if(view == null)
            {
                view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == 0);
            }

            view.UpdateEquipmentInfo("other", equipmentSetInfo);
        }
    }
    private void OnPlayerLeave(int id)
    {
        var view = PlayerEquipmentSetViews.FirstOrDefault(x => x.PlayerId == id);
        view.SetAsEmpty();
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
}

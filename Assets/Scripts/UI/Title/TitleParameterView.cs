using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using System.Linq;

public class TitleParameterView : MonoBehaviour
{
    [SerializeField] TitleParameterCell HpParameterCell;
    [SerializeField] TitleParameterCell MaxSpeedParameterCell;
    [SerializeField] TitleParameterCell HeatParameterCell;
    private void Awake()
    {
        PlayerEquipmentModel.GetInstance()
        .PlayerEquipmentUpdateObservable()
        .Subscribe(equipmentInfo => OnSelfEquipmentUpdate(equipmentInfo))
        .AddTo(this);

        OnSelfEquipmentUpdate(PlayerEquipmentModel.GetInstance().SelfEquipmentSetInfo);
    }
    public void OnSelfEquipmentUpdate(EquipmentSetInfo equipmentSetInfo)
    {
        var vechielParameter = ParameterHolder.Instance.VehcileParameters.FirstOrDefault(x => x.Type == equipmentSetInfo.Vehicle);
        var maxSpeed = vechielParameter.MaxSpeed;
        MaxSpeedParameterCell.UpdateValue(maxSpeed);
        var saddleHeatRate = ParameterHolder.Instance.SaddleParameters.FirstOrDefault(x => x.Type == equipmentSetInfo.Saddle).HeatRate;
        HeatParameterCell.UpdateValue(saddleHeatRate);
        var maxHp = HealthPoint.MaxPoint + ParameterHolder.Instance.CharaParameters.FirstOrDefault(x => x.Type == equipmentSetInfo.Character).AppendHP;
        HpParameterCell.UpdateValue(maxHp);
    }
}

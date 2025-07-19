using System.Collections.Generic;
using System.Linq;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerEquipmentModel : SingletonBase<PlayerEquipmentModel>
{
    public EquipmentSetInfo SelfEquipmentSetInfo;
    public List<EquipmentSetInfo> PlayerEquipmentSetInfos = new List<EquipmentSetInfo>();
    private readonly Subject<EquipmentSetInfo> _playerEquipmentUpdateSubject = new Subject<EquipmentSetInfo>();
    public Observable<EquipmentSetInfo> PlayerEquipmentUpdateObservable() => _playerEquipmentUpdateSubject;
    private readonly Subject<int> _playerLeaveSubject = new Subject<int>();
    public Observable<int> PlayerLeaveObservable() => _playerLeaveSubject;
    public void Reset()
    {
        SelfEquipmentSetInfo = null;
        PlayerEquipmentSetInfos.Clear();
    }
    public void OnPlayerJoined(int playerId, bool isSelf)
    {
        if(PlayerEquipmentSetInfos.Any(x => x.PlayerId == playerId))
        {
            Debug.LogError($"装備Cacheに同じIdのPlayerが参加しようとしている");
            return;
        }

        if(isSelf)
        {
            SelfEquipmentSetInfo = new EquipmentSetInfo()
            {
                PlayerId = playerId,
            };
        }

        var info = SelfEquipmentSetInfo.Clone();
        PlayerEquipmentSetInfos.Add(info);
    }
    public void OnPlayerLeaved(int playerId)
    {
        PlayerEquipmentSetInfos.RemoveAll(x => x.PlayerId == playerId);
        _playerLeaveSubject.OnNext(playerId);
    }
    public void RequestChangeSelfChara()
    {
        var index = (int)SelfEquipmentSetInfo.Character;
        var nextIndex = (index + 1) % (int)Characters.Max;
        SelfEquipmentSetInfo.Character = (Characters)nextIndex;
        _playerEquipmentUpdateSubject.OnNext(SelfEquipmentSetInfo);
    }
    public void RequestChangeSelfSaddle()
    {
        var index = (int)SelfEquipmentSetInfo.Saddle;
        var nextIndex = (index + 1) % (int)SaddleType.Max;
        SelfEquipmentSetInfo.Saddle = (SaddleType)nextIndex;
        _playerEquipmentUpdateSubject.OnNext(SelfEquipmentSetInfo);
    }
    public void RequestChangeSelfVehicle()
    {
        var index = (int)SelfEquipmentSetInfo.Vehicle;
        var nextIndex = (index + 1) % (int)Vehicles.Max;
        SelfEquipmentSetInfo.Vehicle = (Vehicles)nextIndex;
        _playerEquipmentUpdateSubject.OnNext(SelfEquipmentSetInfo);
    }
    public void UpdateSelfEquipmen(EquipmentSetInfo equipmentSetInfo)
    {
        SelfEquipmentSetInfo = equipmentSetInfo.Clone();

        Rpc_BroadcastEquipment(equipmentSetInfo.PlayerId, equipmentSetInfo.Character, equipmentSetInfo.Saddle, equipmentSetInfo.Vehicle);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_BroadcastEquipment(int playerId, Characters chara, SaddleType saddle, Vehicles vehicle)
    {
        var equipmentInfo = PlayerEquipmentSetInfos.FirstOrDefault(x => x.PlayerId == playerId);

        if (equipmentInfo == null)
        {
            Debug.LogError($"Playerの装備が見つからない {playerId}");
            return;
        }

        equipmentInfo.Character = chara;
        equipmentInfo.Saddle = saddle;
        equipmentInfo.Vehicle = vehicle;

        _playerEquipmentUpdateSubject.OnNext(equipmentInfo);
    }
}

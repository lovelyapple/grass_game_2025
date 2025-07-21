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

            var info = SelfEquipmentSetInfo.Clone();
            PlayerEquipmentSetInfos.Add(info);
        }
        else
        {
            var equipmentInfo = new EquipmentSetInfo() { PlayerId = playerId };
            PlayerEquipmentSetInfos.Add(equipmentInfo);
            _playerEquipmentUpdateSubject.OnNext(equipmentInfo);
        }
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
    public void ShuffleSelfEquipment()
    {
        var chara = GetEnumRandom((int)Characters.Max);
        var saddle = GetEnumRandom((int)SaddleType.Max);
        var Vehicle = GetEnumRandom((int)Vehicles.Max);

        SelfEquipmentSetInfo.Character = (Characters)chara;
        SelfEquipmentSetInfo.Saddle = (SaddleType)saddle;
        SelfEquipmentSetInfo.Vehicle = (Vehicles)Vehicle;
        _playerEquipmentUpdateSubject.OnNext(SelfEquipmentSetInfo);
    }
    private int GetEnumRandom(int maxEnumCount)
    {
        var result = UnityEngine.Random.Range(0, maxEnumCount);
        return result;
    }
    public void SaveSelfEquipment()
    {
        RpcConnector.Instance.Rpc_BroadcastEquipmentSave(
        SelfEquipmentSetInfo.PlayerId, 
        SelfEquipmentSetInfo.Character,
        SelfEquipmentSetInfo.Saddle,
        SelfEquipmentSetInfo.Vehicle);

        GamePlayerInfoModel.GetInstance().UpdateSelfEquipment(SelfEquipmentSetInfo);
    }
    public void OnReceivePlayerEquipSave(EquipmentSetInfo info)
    {
        if(info.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            return;
        }

        Debug.Log($"player equipment updated {info.PlayerId}");
        var equipmentInfo = PlayerEquipmentSetInfos.FirstOrDefault(x => x.PlayerId == info.PlayerId);

        if (equipmentInfo == null)
        {
            Debug.LogError($"Playerの装備が見つからない {info.PlayerId}");
            return;
        }

        equipmentInfo.Character = info.Character;
        equipmentInfo.Saddle = info.Saddle;
        equipmentInfo.Vehicle = info.Vehicle;

        _playerEquipmentUpdateSubject.OnNext(equipmentInfo);
    }
}

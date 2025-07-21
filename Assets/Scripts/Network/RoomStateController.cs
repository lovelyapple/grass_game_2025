using System;
using System.Collections.Generic;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
public struct PlayerEquipmentSetInfoStruct : INetworkStruct
{
    public int PlayerId;
    public int Character;
    public int SaddleType;
    public int Vehicle;
}
public class RoomStateController : NetworkBehaviour
{
    // ネットワークで共有される変数
    [Networked] public int CurrentRoomPhase { get; set; }
    // 一般ユーザーがこれを使ってAdminを取得
    [Networked] public int AdminId { get; private set; }

    // todoこいつを消す
    [Networked] public NetworkDictionary<int, PlayerEquipmentSetInfoStruct> RoomPlayerEquipmentCache => default;
    public static RoomStateController Instance;
    public void Awake()
    {
        Instance = this;

        RoomModel.GetInstance().OnPlayerJoinObeservable()
        .Subscribe(x => OnPlayerJoin(x.Item1))
        .AddTo(this);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(x => OnPlayerLeave(x))
        .AddTo(this);

        PlayerEquipmentModel.GetInstance().PlayerEquipmentUpdateObservable()
        .Subscribe(x => OnPlayerUpdateEquipmentInfo(x))
        .AddTo(this);
    }
    public void UpdateCurrentRoomPhaseAdmin(RoomPhase roomPhase)
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        CurrentRoomPhase = (int)roomPhase;
    }

    public void SetupId(int id)
    {
        if (GameCoreModel.Instance.IsAdminUser)
        {
            AdminId = id;
        }
        else
        {
            RoomModel.GetInstance().OnAdminSpawned(id);
        }
    }

    private void Update()
    {
        if (RoomModel.GetInstance().AdminId == 0 && AdminId != 0)
        {
            RoomModel.GetInstance().OnAdminSpawned(AdminId);
        }
    }
    public void OnPlayerJoin(int playerId)
    {
        if(AdminId == playerId || !GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        RoomPlayerEquipmentCache.Add(playerId, new PlayerEquipmentSetInfoStruct() { PlayerId = playerId });
    }
    public void OnPlayerLeave(int playerId)
    {
        if (AdminId == playerId || !GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        RoomPlayerEquipmentCache.Remove(playerId);
    }
    public void OnPlayerUpdateEquipmentInfo(EquipmentSetInfo info)
    {
        if (GameCoreModel.Instance.IsAdminUser)
        {
            var structInfo = new PlayerEquipmentSetInfoStruct();
            structInfo.PlayerId = (int)info.PlayerId;
            structInfo.Character = (int)info.Character;
            structInfo.SaddleType = (int)info.Saddle;
            structInfo.Vehicle = (int)info.Vehicle;

            RoomPlayerEquipmentCache.Remove(info.PlayerId);
            RoomPlayerEquipmentCache.Add(info.PlayerId, structInfo);
        }
    }
    public override void Spawned()
    {
        base.Spawned();
        RoomModel.GetInstance().OnRoomStateControllerSpawn(this);
    }
}

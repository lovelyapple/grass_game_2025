using System;
using System.Collections.Generic;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
public struct EquipmentSetInfoStruct : INetworkStruct
{
    public int PlayerId;
    public int Character;
    public int SaddleType;
    public int Vehicle;
}
public class RoomStateController : NetworkBehaviour
{
    // ネットワークで共有される変数
    // [Networked]
    public RoomPhase CurrentRoomPhase { get; set; }
    // 一般ユーザーがこれを使ってAdminを取得
    [Networked] public int AdminId { get; private set; }
    [Networked] public NetworkDictionary<int, EquipmentSetInfoStruct> RoomPlayerEuipmentCache => default;
    public static RoomStateController Instance;
    public void Awake()
    {
        Instance = this;
        RoomModel.GetInstance().OnRoomStateControllerSpawn(this);

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
    public void UpdateCurrentRoomPhase(RoomPhase roomPhase)
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        CurrentRoomPhase = roomPhase;
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

        RoomPlayerEuipmentCache.Add(playerId, new EquipmentSetInfoStruct() { PlayerId = playerId });
    }
    public void OnPlayerLeave(int playerId)
    {
        if (AdminId == playerId || !GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        RoomPlayerEuipmentCache.Remove(playerId);
    }
    public void OnPlayerUpdateEquipmentInfo(EquipmentSetInfo info)
    {
        if (GameCoreModel.Instance.IsAdminUser)
        {
            var structInfo = new EquipmentSetInfoStruct();
            structInfo.PlayerId = (int)info.PlayerId;
            structInfo.Character = (int)info.Character;
            structInfo.SaddleType = (int)info.Saddle;
            structInfo.Vehicle = (int)info.Vehicle;

            RoomPlayerEuipmentCache.Remove(info.PlayerId);
            RoomPlayerEuipmentCache.Add(info.PlayerId, structInfo);
        }
    }
}

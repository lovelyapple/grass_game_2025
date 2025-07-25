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
    [Networked, OnChangedRender(nameof(RoomPhaseChanged))] 
    public int CurrentRoomPhase { get; set; }
    // 一般ユーザーがこれを使ってAdminを取得
    [Networked]  
    public int AdminId { get; private set; }
    public static RoomStateController Instance;
    public override void Spawned()
    {
        base.Spawned();
        RoomModel.GetInstance().OnRoomStateControllerSpawn(this);
        ModelCache.Admin.OnRoomStateControllerSpawn(this);
        Instance = this;
    }
    private void RoomPhaseChanged(NetworkBehaviourBuffer previous)
    {
        Debug.Log($"RoomStateController UpdatePhase {(RoomPhase)CurrentRoomPhase}");
        RoomModel.GetInstance().ReceivedRoomPhaseUpdate((RoomPhase)CurrentRoomPhase);
    }
}

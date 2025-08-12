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
    [Networked] 
    public int CurrentRoomPhase { get; set; }
    // 一般ユーザーがこれを使ってAdminを取得
    [Networked]  
    public int AdminId { get; private set; }
    public static RoomStateController Instance;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public override void Spawned()
    {
        base.Spawned();
        RoomModel.GetInstance().OnRoomStateControllerSpawn(this);
        ModelCache.Admin.OnRoomStateControllerSpawn(this);
        Instance = this;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        Instance = null;
    }
}

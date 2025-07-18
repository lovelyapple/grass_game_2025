using System;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class RoomStateController : NetworkBehaviour
{
    // ネットワークで共有される変数
    // [Networked]
    public RoomPhase CurrentRoomPhase { get; set; }
    // 一般ユーザーがこれを使ってAdminを取得
    [Networked] public int AdminId { get; private set; }

    public void Awake()
    {
        RoomModel.GetInstance().OnRoomStateControllerSpawn(this);
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
}

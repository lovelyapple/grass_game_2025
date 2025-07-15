using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class RoomStateController : NetworkBehaviour
{
    // ネットワークで共有される変数
    // [Networked]
    public RoomPhase CurrentRoomPhase { get; set; }

    public void Awake()
    {
        RoomModel.GetInstance().OnAdminJoined(this);
    }
    public void UpdateCurrentRoomPhase(RoomPhase roomPhase)
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        CurrentRoomPhase = roomPhase;
    }
}

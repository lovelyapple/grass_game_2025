using Fusion;
using R3;
using UnityEngine;

public class RoomStateController : NetworkBehaviour
{
    // ネットワークで共有される変数
    [Networked]
    public RoomPhase CurrentRoomPhase { get; set; }

    public void Awake()
    {
        RoomModel.GetInstance().OnAdminJoined(this);
    }
}

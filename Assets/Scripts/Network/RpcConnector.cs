using System;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class RpcConnector : NetworkBehaviour
{
    public static RpcConnector Instance;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_BroadcastEquipmentSave(int playerId, Characters chara, SaddleType saddle, Vehicles vehicle)
    {
        var info = new EquipmentSetInfo()
        {
            PlayerId = playerId,
            Character = chara,
            Saddle = saddle,
            Vehicle = vehicle,
        };

        PlayerEquipmentModel.GetInstance().OnReceivePlayerEquipSave(info);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_BroadcastStartCountDown(double endTimeUnixMilliseconds)
    {
        RoomModel.GetInstance().ReceivedStartCountDown(endTimeUnixMilliseconds);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_BroadcastCancelCountDown(double endTimeUnixMilliseconds)
    {
        RoomModel.GetInstance().ReceiveCountDownCancel(endTimeUnixMilliseconds);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_BroadcastRoomPhase(RoomPhase roomPhase)
    {
        Debug.Log($"RoomStateController UpdatePhase {roomPhase}");
        RoomModel.GetInstance().ReceivedRoomPhaseUpdate(roomPhase);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_BroadcastFinishLine(int playerId)
    {
        ModelCache.Admin.OnPlayerFinishedLine(playerId);
    }
}

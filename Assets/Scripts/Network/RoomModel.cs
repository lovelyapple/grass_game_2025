using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
public class PlayerInfo
{
    public readonly PlayerRef PlayerRef;
    public VehicleBase Vehicle;
    public PlayerInfo(PlayerRef playerRef)
    {
        PlayerRef = playerRef;
    }
}
public class RoomModel : SingletonBase<RoomModel>
{
    private RoomStateController _roomStateController;   
    public bool HasAdmin => _roomStateController != null;
    private ReactiveProperty<RoomPhase> _roomPhaseChangedSubject = new ReactiveProperty<RoomPhase>(RoomPhase.Waiting);
    public Observable<RoomPhase> RoomPhaseChangedObseverble() => _roomPhaseChangedSubject;
    private List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
    public void OnAdminJoined(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        UpdateRoomPhase(RoomPhase.Waiting);
    }
    public void OnPlayerJoined(PlayerRef playerRef)
    {
        var prevInfo = PlayerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if(prevInfo != null)
        {
            Debug.LogError("same playerRef ?!?!");
            return;
        }

        PlayerInfos.Add(new PlayerInfo(playerRef));
        Debug.Log($"player joined id {playerRef.PlayerId} {playerRef.RawEncoded}");
    }
    public void OnPlayerLeaved(PlayerRef playerRef)
    {
        var prevInfo = PlayerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if (prevInfo != null)
        {
            Debug.LogError("same playerRef ?!?!");
            return;
        }

        PlayerInfos.Remove(prevInfo);
        Debug.Log($"player leaved id {playerRef.PlayerId} {playerRef.RawEncoded}");
    }
    public void UpdateRoomPhase(RoomPhase roomPhase)
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        _roomStateController?.UpdateCurrentRoomPhase(roomPhase);
        _roomPhaseChangedSubject.Value = roomPhase;
    }
}

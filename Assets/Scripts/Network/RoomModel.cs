using System;
using R3;
using UnityEngine;

public class RoomModel : SingletonBase<RoomModel>
{
    private RoomStateController _roomStateController;
    public bool HasAdmin => _roomStateController != null;
    private ReactiveProperty<RoomPhase> _roomPhaseChangedSubject = new ReactiveProperty<RoomPhase>(RoomPhase.Waiting);
    public Observable<RoomPhase> RoomPhaseChangedObseverble() => _roomPhaseChangedSubject;
    public void OnAdminJoined(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
    }
    public void UpdateRoomPhase(RoomPhase roomPhase)
    {
        _roomPhaseChangedSubject.Value = roomPhase;
    }
}

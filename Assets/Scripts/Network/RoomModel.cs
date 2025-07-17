using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    private string _roomName;
    public void SetupRoomId(string roomName)
    {
        _roomName = roomName;
        Debug.LogWarning($"room model update room {roomName}");
    }
    public void OnAdminJoined(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        UpdateRoomPhase(RoomPhase.Waiting);

        if (GameCoreModel.Instance.IsAdminUser)
        {
            _playerCountRequesting = PlayerInfos.Count;
            RequestUpdateRoom(_roomName).Forget();
        }
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

        if(GameCoreModel.Instance.IsAdminUser)
        {
            _playerCountRequesting = PlayerInfos.Count;
            RequestUpdateRoom(_roomName).Forget();
        }
    }
    private bool _isSendRequestUpdateRoom = false;
    private int? _playerCountRequesting = null;
    private async UniTask<Unit> RequestUpdateRoom(string roomName)
    {
        if(_isSendRequestUpdateRoom) 
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoom = true;

        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            var roomPhase = UpdateRoomPhase(playerCount).ToString();
            await RoomService.UpdateRoom(new RoomInfo(_roomName, playerCount, roomPhase), new CancellationToken());
        }

        _isSendRequestUpdateRoom = false;
        return Unit.Default;
    }
    public void OnPlayerLeaved(PlayerRef playerRef)
    {
        var prevInfo = PlayerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if (prevInfo == null)
        {
            Debug.LogError("playerRef not found ?!?!");
            return;
        }

        PlayerInfos.Remove(prevInfo);
        Debug.Log($"player leaved id {playerRef.PlayerId} {playerRef.RawEncoded}");

        if (GameCoreModel.Instance.IsAdminUser)
        {
            var playerCount = PlayerInfos.Count;
            var roomPhase = UpdateRoomPhase(playerCount).ToString();
            RoomService.UpdateRoom(new RoomInfo(_roomName, playerCount, roomPhase), new CancellationToken()).Forget();
        }
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
    private RoomPhase UpdateRoomPhase(int member)
    {
        if(member > GameConstant.GameStartPlayerCount)
        {
            return RoomPhase.CountDown;
        }

        return RoomPhase.Waiting;
    }
}

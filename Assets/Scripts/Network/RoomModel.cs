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
    public bool HasRoomStateCtrl => _roomStateController != null;
    private ReactiveProperty<RoomPhase> _roomPhaseChangedSubject = new ReactiveProperty<RoomPhase>(RoomPhase.Waiting);
    public Observable<RoomPhase> RoomPhaseChangedObservable() => _roomPhaseChangedSubject;
    private List<PlayerInfo> _playerInfos = new List<PlayerInfo>();
    private string _roomName;
    
    
    // Admin用のPlayerPref
    private PlayerRef _adminPlayerRef;
    #region NetworkCallBack
    public void SetupRoomId(string roomName)
    {
        _roomName = roomName;
        Debug.LogWarning($"room model update room {roomName}");
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        UpdateRoomPhase(RoomPhase.Waiting);
    }
    public void OnPlayerJoined(PlayerRef playerRef, bool isSelf)
    {
        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if(prevInfo != null)
        {
            Debug.LogError("same playerRef ?!?!");
            return;
        }

        _playerInfos.Add(new PlayerInfo(playerRef));
        Debug.Log($"player joined id {playerRef.PlayerId} {playerRef.RawEncoded}");

        if(GameCoreModel.Instance.IsAdminUser)
        {
            if (isSelf)
            {
                _adminPlayerRef = playerRef;
            }

            RequestUpdateRoom(_roomName).Forget();
        }
        else
        {
            // この段階Amdinのstateまた生成されていない...
            // if (_playerInfos.Count == 1)
            // {
            //     NetworkRunnerController.Runner.Shutdown();
            //     Debug.LogError($"Adminがいない部屋にはいった、退室します");      
            // }
        }
    }
    public void OnPlayerLeaved(PlayerRef playerRef)
    {
        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if (prevInfo == null)
        {
            Debug.LogError("playerRef not found ?!?!");
            return;
        }

        _playerInfos.Remove(prevInfo);
        Debug.Log($"player leaved id {playerRef.PlayerId} {playerRef.RawEncoded}");

        if (GameCoreModel.Instance.IsAdminUser)
        {
            RequestUpdateRoom(_roomName).Forget();
        }
        else
        {
            if (_roomStateController == null || _roomStateController.AdminId == playerRef.PlayerId)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがlogoutした為、部屋が閉じられた");
            }       
        }
    }
    #endregion
    private bool _isSendRequestUpdateRoom = false;
    private int? _playerCountRequesting = null;
    private async UniTask<Unit> RequestUpdateRoom(string roomName)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if(_isSendRequestUpdateRoom) 
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoom = true;

        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            var roomPhase = GetRoomPhaseFromMember(playerCount).ToString();
            await RoomService.UpdateRoom(new RoomInfo(_roomName, playerCount, roomPhase), new CancellationToken());
        }

        _isSendRequestUpdateRoom = false;
        return Unit.Default;
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
    private RoomPhase GetRoomPhaseFromMember(int member)
    {
        if(member > GameConstant.GameStartPlayerCount)
        {
            return RoomPhase.CountDown;
        }

        return RoomPhase.Waiting;
    }

    private int GetCurrentPlayerCount()
    {
        if (GameCoreModel.Instance.IsAdminUser)
        {
            return _playerInfos.Count(x => x.PlayerRef != _adminPlayerRef);
        }

        if (_roomStateController == null || _roomStateController.AdminId == 0)
        {
            Debug.LogError($"admin が存在しない");
            return -1;
        }

        return _playerInfos.Count(x => x.PlayerRef.PlayerId != _roomStateController.AdminId);
    }
}

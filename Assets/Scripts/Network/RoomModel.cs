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

    private readonly Subject<(int, bool)> _onPlayerJoinSubject = new Subject<(int, bool)>();
    public Observable<(int, bool)> OnPlayerJoinObeservable() => _onPlayerJoinSubject;

    private readonly Subject<int> _onPlayerLeaveSubject = new Subject<int>();
    public Observable<int> OnPlayerLeaveObservable() => _onPlayerLeaveSubject;

    private List<PlayerInfo> _playerInfos = new List<PlayerInfo>();
    public string RoomName{ get; private set; }
    public PlayerRef SelfPlayerRef{ get; private set; }
    
    // Adminj自身はこれを使う
    private PlayerRef _adminPlayerRef;
    public int AdminId { get; private set; }
    #region NetworkCallBack
    public void OnSelfJoinedRoom(string roomName, PlayerRef playerRef)
    {
        RoomName = roomName;
        SelfPlayerRef = playerRef;
        Debug.LogWarning($"room model update room {roomName}");
    }

    public void OnAdminSpawned(int adminId)
    {
        AdminId = adminId;
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

            RequestUpdateRoom(RoomName).Forget();
        }
        else
        {
            // この段階Amdinのstateまた生成されていない...
            if (NetworkRunnerController.Runner.SessionInfo.PlayerCount == 1)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがいない部屋にはいった、退室します");      
            }
        }

        _onPlayerJoinSubject.OnNext((playerRef.PlayerId, isSelf));
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
            RequestUpdateRoom(RoomName).Forget();
        }
        else
        {
            if (_roomStateController == null || AdminId == playerRef.PlayerId)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがlogoutした為、部屋が閉じられた");
            }       
        }

        _onPlayerLeaveSubject.OnNext(playerRef.PlayerId);
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
            await RoomService.UpdateRoom(new RoomInfo(RoomName, playerCount, roomPhase), new CancellationToken());
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

        if (_roomStateController == null || AdminId == 0)
        {
            Debug.LogError($"admin が存在しない");
            return -1;
        }

        return _playerInfos.Count(x => x.PlayerRef.PlayerId != AdminId);
    }
}

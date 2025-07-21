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
        UpdateRoomPhaseAdmin(RoomPhase.Waiting);
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

            RequestUpdateRoomAdmin(RoomName).Forget();
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
            RequestUpdateRoomAdmin(RoomName).Forget();
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
    public RoomPhase CurrentRoomPhase{ get; private set; }
    private bool _isSendRequestUpdateRoom = false;
    private int? _playerCountRequesting = null;
    private async UniTask<Unit> RequestUpdateRoomAdmin(string roomName)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if(_isSendRequestUpdateRoom) 
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoom = true;

        var prevRoomPhase = CurrentRoomPhase;
        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            CurrentRoomPhase = GetRoomPhaseFromMember(playerCount);
            await RoomService.UpdateRoom(new RoomInfo(RoomName, playerCount, CurrentRoomPhase.ToString()), new CancellationToken());
        }

        _isSendRequestUpdateRoom = false;

        if (prevRoomPhase != CurrentRoomPhase)
        {
            if (CurrentRoomPhase == RoomPhase.CountDown)
            {
                StartCountDownAdmin();
            }
            else if(CurrentRoomPhase == RoomPhase.Waiting)
            {
                CancelCountDownAdmin();
            }
        }

        return Unit.Default;
    }
    public void UpdateRoomPhaseAdmin(RoomPhase roomPhase)
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        _roomStateController?.UpdateCurrentRoomPhaseAdmin(roomPhase);
    }
    private RoomPhase GetRoomPhaseFromMember(int member)
    {
        if(CurrentRoomPhase == RoomPhase.Waiting)
        {
            if (member >= GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.CountDown;
            }
        }
        else if(CurrentRoomPhase == RoomPhase.CountDown)
        {
            if (member < GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.Waiting;
            }
        }

        return CurrentRoomPhase;
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
    private DateTime GameStartAtTime;
    private IDisposable _countdownSubscription;
    public double RemainSeconds { get; private set; }
    public double TotalCostTimeSeconds { get; private set; }
    private readonly Subject<Unit> _onRoomCountDownStart = new Subject<Unit>();
    public Observable<Unit> OnRoomCountDownStartObservable() => _onRoomCountDownStart;
    private readonly Subject<double> _countDownChangedSubject = new Subject<double>();
    public Observable<double> OnCountDownChangedAsObservable() => _countDownChangedSubject;
    private readonly Subject<Unit> _countDownFinishedSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDowFinishedAsObservable() => _countDownFinishedSubject;
    private readonly Subject<Unit> _countDownCancelSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDownCancelAsObservable() => _countDownCancelSubject;
    private void StartCountDownAdmin()
    {
        if(!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        var endTime = DateTime.UtcNow.AddSeconds(GameConstant.CountDownSec);
        double unixTimeMs = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();
        RpcConnector.Instance.Rpc_BroadcastStartCountDown(unixTimeMs);
    }
    public void ReceivedStartCountDown(double endTimeUnixMilliseconds)
    {
        _countdownSubscription?.Dispose();
        _onRoomCountDownStart.OnNext(Unit.Default);
        RoomStateController.Instance.UpdateCurrentRoomPhaseAdmin(RoomPhase.CountDown);
        PerformCountDownAsync(endTimeUnixMilliseconds);
    }
    public void PerformCountDownAsync(double endTimeUnixMilliseconds)
    {
        GameStartAtTime = DateTimeOffset.FromUnixTimeMilliseconds((long)endTimeUnixMilliseconds).UtcDateTime;
        RemainSeconds = (GameStartAtTime - DateTime.Now).TotalSeconds;

        _countdownSubscription = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .TakeWhile(_ => RemainSeconds > 0)
            .Subscribe(_ =>
            {
                RemainSeconds--;

                if (RemainSeconds <= 0)
                {
                    _countDownFinishedSubject.OnNext(Unit.Default);
                }
                else
                {
                    _countDownChangedSubject.OnNext(RemainSeconds);
                }
            });
    }
    private void CancelCountDownAdmin()
    {
        if (!GameCoreModel.Instance.IsAdminUser)
        {
            return;
        }

        RpcConnector.Instance.Rpc_BroadcastCancelCountDown(0);
    }
    public void ReceiveCountDownCancle(double time)
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
        _countDownCancelSubject.OnNext(Unit.Default);
        RoomStateController.Instance.UpdateCurrentRoomPhaseAdmin(RoomPhase.Waiting);
    }
}

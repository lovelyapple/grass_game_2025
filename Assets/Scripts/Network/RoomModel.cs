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
    private int _adminId;
    private RoomPhase _currentRoomPhase;
    public string RoomName{ get; private set; }
    public PlayerRef SelfPlayerRef { get; private set; }

    private Subject<RoomPhase> _roomPhaseUpdateSubject = new Subject<RoomPhase>();
    public Observable<RoomPhase> RoomPhaseUpdateObservable() => _roomPhaseUpdateSubject;
    public void ReceivedRoomPhaseUpdate(RoomPhase roomPhase)
    {
        _currentRoomPhase = roomPhase;
        _roomPhaseUpdateSubject.OnNext(roomPhase);
    }
    public void ReceivedUpdateAdminId(int playerId)
    {
        _adminId = playerId;
    }
    public void OnSelfJoinedRoom(string roomName, PlayerRef playerRef)
    {
        RoomName = roomName;
        SelfPlayerRef = playerRef;
        Debug.LogWarning($"room model update room {roomName}");
    }
    

    private RoomStateController _roomStateController;   

    private readonly Subject<(int, bool)> _onPlayerJoinSubject = new Subject<(int, bool)>();
    public Observable<(int, bool)> OnPlayerJoinObservable() => _onPlayerJoinSubject;

    private readonly Subject<int> _onPlayerLeaveSubject = new Subject<int>();
    public Observable<int> OnPlayerLeaveObservable() => _onPlayerLeaveSubject;

    private List<PlayerInfo> _playerInfos = new List<PlayerInfo>();

    // Adminj自身はこれを使う
    private PlayerRef _adminPlayerRef;

    #region NetworkCallBack

    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
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
            if (_roomStateController == null || _adminId == playerRef.PlayerId)
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
    private async UniTask<Unit> RequestUpdateRoomAdmin(string roomName)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if(_isSendRequestUpdateRoom) 
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoom = true;

        var prevRoomPhase = _currentRoomPhase;
        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            _currentRoomPhase = GetRoomPhaseFromMember(playerCount);
            await RoomService.UpdateRoom(new RoomInfo(RoomName, playerCount, _currentRoomPhase.ToString()), new CancellationToken());
        }

        _isSendRequestUpdateRoom = false;

        if (prevRoomPhase != _currentRoomPhase)
        {
            if (_currentRoomPhase == RoomPhase.CountDown)
            {
                StartCountDownAdmin();
            }
            else if(_currentRoomPhase == RoomPhase.Waiting)
            {
                CancelCountDownAdmin();
            }
        }

        return Unit.Default;
    }
    private RoomPhase GetRoomPhaseFromMember(int member)
    {
        if(_currentRoomPhase == RoomPhase.Waiting)
        {
            if (member >= GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.CountDown;
            }
        }
        else if(_currentRoomPhase == RoomPhase.CountDown)
        {
            if (member < GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.Waiting;
            }
        }

        return _currentRoomPhase;
    }

    private int GetCurrentPlayerCount()
    {
        if (GameCoreModel.Instance.IsAdminUser)
        {
            return _playerInfos.Count(x => x.PlayerRef != _adminPlayerRef);
        }

        if (_roomStateController == null || _adminId == 0)
        {
            Debug.LogError($"admin が存在しない");
            return -1;
        }

        return _playerInfos.Count(x => x.PlayerRef.PlayerId != _adminId);
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
    }
}

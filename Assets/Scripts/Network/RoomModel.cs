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
    public string RoomName { get; private set; }
    public PlayerRef SelfPlayerRef { get; private set; }
    private RoomStateController _roomStateController;
    private int AdmingId => _roomStateController != null ? _roomStateController.AdminId : 0;
    private List<PlayerInfo> _playerInfos = new List<PlayerInfo>();

    private readonly Subject<(int, bool)> _onPlayerJoinSubject = new Subject<(int, bool)>();
    public Observable<(int, bool)> OnPlayerJoinObservable() => _onPlayerJoinSubject;

    private Subject<RoomPhase> _roomPhaseUpdateSubject = new Subject<RoomPhase>();
    public Observable<RoomPhase> RoomPhaseUpdateObservable() => _roomPhaseUpdateSubject;

    private readonly Subject<int> _onPlayerLeaveSubject = new Subject<int>();
    public Observable<int> OnPlayerLeaveObservable() => _onPlayerLeaveSubject;


    #region NetworkCallBack
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
    }
    public void OnSelfJoinedRoom(string roomName, PlayerRef playerRef)
    {
        RoomName = roomName;
        SelfPlayerRef = playerRef;
        Debug.LogWarning($"room model update room {roomName}");
    }
    // tracker.join → (infoObj.create → rootObj.Register) → here → admin.join
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject)
    {
        if (!GameCoreModel.Instance.IsAdminUser)
        {
            // この段階Amdinのstateまた生成されていない...
            if (NetworkRunnerController.Runner.SessionInfo.PlayerCount == 1)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがいない部屋にはいった、退室します");
                return;
            }
        }

        var playerRef = infoObject.PlayerRef;
        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if(prevInfo != null)
        {
            Debug.LogError("same playerRef ?!?!");
            return;
        }

        var isSelf = playerRef.PlayerId == SelfPlayerRef.PlayerId;
        _playerInfos.Add(new PlayerInfo(playerRef));
        Debug.Log($"player joined id {playerRef.PlayerId} {playerRef.RawEncoded}");

        _onPlayerJoinSubject.OnNext((playerRef.PlayerId, isSelf));
        ModelCache.Admin.OnPlayerInfoObjectJoined(infoObject);
    }
    // tackter.leave → here → playerRoot.leave → admin.Leave
    public void OnPlayerLeaved(PlayerRef playerRef)
    {
        if (!GameCoreModel.Instance.IsAdminUser)
        {
            if (_roomStateController == null || AdmingId == playerRef.PlayerId)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがlogoutした為、部屋が閉じられた");
                return;
            }
        }

        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerRef == playerRef);

        if (prevInfo == null)
        {
            Debug.LogError("playerRef not found ?!?!");
            return;
        }

        _playerInfos.Remove(prevInfo);
        Debug.Log($"player leaved id {playerRef.PlayerId} {playerRef.RawEncoded}");
        _onPlayerLeaveSubject.OnNext(playerRef.PlayerId);
    }
    #endregion
    #region count_down
    private DateTime GameStartAtTime;
    private IDisposable _countdownSubscription;
    public double RemainSeconds { get; private set; }
    private readonly Subject<Unit> _onRoomCountDownStart = new Subject<Unit>();
    public Observable<Unit> OnRoomCountDownStartObservable() => _onRoomCountDownStart;
    private readonly Subject<double> _countDownChangedSubject = new Subject<double>();
    public Observable<double> OnCountDownChangedAsObservable() => _countDownChangedSubject;
    private readonly Subject<Unit> _countDownFinishedSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDowFinishedAsObservable() => _countDownFinishedSubject;
    private readonly Subject<Unit> _countDownCancelSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDownCancelAsObservable() => _countDownCancelSubject;
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
    #endregion
    #region Rpc_CallReceiver
    public void ReceivedRoomPhaseUpdate(RoomPhase roomPhase)
    {
        Debug.Log($"ReceivedRoomPhaseUpdate {roomPhase}");
        _roomPhaseUpdateSubject.OnNext(roomPhase);
    }
    public void ReceivedStartCountDown(double endTimeUnixMilliseconds)
    {
        _countdownSubscription?.Dispose();
        _onRoomCountDownStart.OnNext(Unit.Default);
        PerformCountDownAsync(endTimeUnixMilliseconds);
    }
    public void ReceiveCountDownCancle(double time)
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
        _countDownCancelSubject.OnNext(Unit.Default);
    }
    #endregion

}

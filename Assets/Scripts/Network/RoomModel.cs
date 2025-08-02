using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
public class RoomModel : SingletonBase<RoomModel>
{
    private class PlayerInfo
    {
        public PlayerRef Ref;
        public PlayerInfo(PlayerRef playerRef)
        {
            Ref = playerRef;
        }
        public int PlayerId => Ref.PlayerId;
    }
    public string RoomName { get; private set; }
    public PlayerRef SelfPlayerRef { get; private set; }
    private RoomStateController _roomStateController;
    private int _admingId;
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
        _admingId = _roomStateController.AdminId;
    }
    public void OnSelfJoinedRoom(string roomName, PlayerRef playerRef)
    {
        RoomName = roomName;
        SelfPlayerRef = playerRef;
        Debug.LogWarning($"room model update room {roomName} Self Ref {SelfPlayerRef.PlayerId}");
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
        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerId == playerRef.PlayerId);

        if(prevInfo != null)
        {
            Debug.LogError($"same playerRef ?!?! {playerRef.PlayerId}");
            return;
        }

        var isSelf = playerRef.PlayerId == SelfPlayerRef.PlayerId;
        _playerInfos.Add(new PlayerInfo(playerRef));
        Debug.Log($"player joined id {playerRef.PlayerId} {playerRef.RawEncoded}");

        _onPlayerJoinSubject.OnNext((playerRef.PlayerId, isSelf));
        ModelCache.Admin.OnPlayerInfoObjectJoined(infoObject);

        if(!GameCoreModel.Instance.IsAdminUser)
        {
            if(infoObject.PlayerId == SelfPlayerRef.PlayerId)
            {
                PlayerRootObject.Instance.SelfInfoObject = infoObject;
            }
        }
    }
    // tackter.leave → here → playerRoot.leave → admin.Leave
    public void OnPlayerLeaved(PlayerRef playerRef)
    {
        if (!GameCoreModel.Instance.IsAdminUser)
        {
            if (_roomStateController == null || _admingId == playerRef.PlayerId)
            {
                NetworkRunnerController.Runner.Shutdown();
                Debug.LogError($"Adminがlogoutした為、部屋が閉じられた");
                return;
            }
        }

        var prevInfo = _playerInfos.FirstOrDefault(x => x.PlayerId == playerRef.PlayerId);

        if (prevInfo == null)
        {
            Debug.LogError($"playerRef not found ?!?! id:{playerRef.PlayerId}");
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

    private readonly Subject<int> _countDownUpdatedSubject = new Subject<int>();
    public Observable<int> OnCountDownUpdatedAsObservable() => _countDownUpdatedSubject;

    private readonly Subject<Unit> _countDownFinishedSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDowFinishedAsObservable() => _countDownFinishedSubject;

    private readonly Subject<Unit> _countDownCancelledSubject = new Subject<Unit>();
    public Observable<Unit> OnCountDownCancelledAsObservable() => _countDownCancelledSubject;
    private void PerformCountDownAsync(double endTimeUnixMilliseconds)
    {
        GameStartAtTime = DateTimeOffset.FromUnixTimeMilliseconds((long)endTimeUnixMilliseconds).UtcDateTime;
        RemainSeconds = (GameStartAtTime - DateTime.UtcNow).TotalSeconds;
        UnityEngine.Debug.LogWarning($"Start CountDown Client {RemainSeconds}");
        _onRoomCountDownStart.OnNext(Unit.Default);

        _countdownSubscription = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .TakeWhile(_ => DateTime.UtcNow < GameStartAtTime)
            .Subscribe(
                onNext: _ =>
                {
                    RemainSeconds = (GameStartAtTime - DateTime.UtcNow).TotalSeconds;
                    _countDownUpdatedSubject.OnNext((int)RemainSeconds);
                    ModelCache.Admin.OnCountDownUpdate(RemainSeconds);
                },
                onCompleted: _ =>
                {
                    ClearCountDownHandler();
                    ModelCache.Admin.OnCountDownFinished();
                    _countDownFinishedSubject.OnNext(Unit.Default);

                }
            );
    }
    private void ClearCountDownHandler()
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
        RemainSeconds = 0;
    }
    #endregion
    #region Rpc_CallReceiver
    public void ReceivedRoomPhaseUpdate(RoomPhase roomPhase)
    {
        Debug.Log($"ReceivedRoomPhaseUpdate {roomPhase}");
        _roomPhaseUpdateSubject.OnNext(roomPhase);

        if(roomPhase == RoomPhase.Playing)
        {
            MatchModel.GetInstance().RequestStartMatchAsync(new System.Threading.CancellationToken()).Forget();
        }
    }
    public void ReceivedStartCountDown(double endTimeUnixMilliseconds)
    {
        ClearCountDownHandler();
        PerformCountDownAsync(endTimeUnixMilliseconds);
    }
    public void ReceiveCountDownCancel(double time)
    {
        ClearCountDownHandler();
        _countDownCancelledSubject.OnNext(Unit.Default);
    }

    #endregion

}

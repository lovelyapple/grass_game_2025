using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using UnityEngine.UI;
public interface IGameAdminModel
{
    public bool IsAdmin { get; }
    public void OnAdminJoined(PlayerRef playerRef);
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController);
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject);
    public void OnPlayerLeave(int playerId);
    public void OnCountDownUpdate(double timeRemain);
    public void OnCountDownFinished();
    public void OnMatchStart();
    public void OnPlayerFinishedLine(int playerId);
    public void OnReturnRoomTop();
    public void UpdateAdminView();
    public void OnPlayerEquipmentConfirm(int playerId);
    public Observable<Unit> RequestUpdateAdminViewObservable();
    public void KickPlayer(int playerId);
}
public class NullGameAdminModel : IGameAdminModel
{
    public bool IsAdmin => false;
    public void OnAdminJoined(PlayerRef playerRef) { }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController) { }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject) { }
    public void OnPlayerLeave(int playerId) { }
    public void OnPlayerEquipmentConfirm(int playerId) { }
    public void OnCountDownUpdate(double timeRemain) { }
    public void OnMatchStart() { }
    public void OnCountDownFinished() { }
    public void OnPlayerFinishedLine(int playerId) { }
    public void OnReturnRoomTop() { }
    private Subject<Unit> _dummySubject = new Subject<Unit>();
    public void UpdateAdminView() { }
    public Observable<Unit> RequestUpdateAdminViewObservable() { return _dummySubject; }
    public void KickPlayer(int playerId) { }
}
public class GameAdminModel : IGameAdminModel
{
    public bool IsAdmin => true;
    private PlayerRef _adminRef;
    private RoomStateController _roomStateController;
    private Dictionary<int, PlayerInfoObject> _playerInfoObjects = new Dictionary<int, PlayerInfoObject>();
    private Dictionary<int, bool> _playerEquipConfirmedDict = new Dictionary<int, bool>();
    private RoomPhase _currentRoomPhase;
    private bool _isSendRequestUpdateRoomPlayerCount = false;
    private bool _isSendRequestUpdateRoomPhase = false;
    private int? _playerCountRequesting = null;
    private Subject<Unit> _requestUpdateAdminView = new Subject<Unit>();
    public Observable<Unit> RequestUpdateAdminViewObservable() { return _requestUpdateAdminView; }
    public void UpdateAdminView() { _requestUpdateAdminView.OnNext(Unit.Default); }
    public void OnAdminJoined(PlayerRef playerRef)
    {
        _adminRef = playerRef;
        AdminPanelController.Open();
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        _roomStateController.AdminId = _adminRef.PlayerId;
        _currentRoomPhase = RoomPhase.Waiting;
        UpdateRoomPhaseOnPlayerJoinLeave(isAdminJoin: true);
        SyncUpdateRoomPhase();
    }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject)
    {
        _playerInfoObjects.Add(infoObject.PlayerId, infoObject);

        var isAdminJoin = infoObject.PlayerRef.PlayerId == _adminRef.PlayerId;

        if(!isAdminJoin)
        {
            _playerEquipConfirmedDict.Add(infoObject.PlayerId, infoObject.IsEquipmentConfirmed);
        }

        UpdateRoomPhaseOnPlayerJoinLeave(isAdminJoin: isAdminJoin);
    }
    public void OnPlayerLeave(int playerId)
    {
        _playerInfoObjects.Remove(playerId);

        if (playerId != _adminRef.PlayerId)
        {
            _playerEquipConfirmedDict.Remove(playerId);
        }

        UpdateRoomPhaseOnPlayerJoinLeave();
    }
    private int? _requestOverrideRoomPhase = null;
    public void OnCountDownUpdate(double timeRemain)
    {
        if (timeRemain <= GameConstant.FinalCountDownSec &&
         _currentRoomPhase == RoomPhase.CountDown)
        {
            RequestUpdateRoomPhase(RoomPhase.CountLock);
        }
    }
    public void OnCountDownFinished()
    {
        RequestUpdateRoomPhase(RoomPhase.MatchLoading);
    }
    public void OnMatchStart()
    {
        _roomStateController.RoomCountDownTime = 0;
        RequestUpdateRoomPhase(RoomPhase.Playing);
    }
    public void OnPlayerFinishedLine(int playerId)
    {
        if (MatchModel.GetInstance().MatchWinner == 0)
        {
            RpcConnector.Instance.Rpc_BroadcastMatchFinished(playerId);
            RequestUpdateRoomPhase(RoomPhase.Result);
        }
    }
    public void OnReturnRoomTop()
    {
        RequestUpdateRoomPhase(RoomPhase.Waiting);
    }
    public void KickPlayer(int playerId)
    {
        RpcConnector.Instance.Rpc_BroadcastKickPlayer(playerId);
    }
    private void UpdateRoomPhaseOnPlayerJoinLeave(bool isAdminJoin = false)
    {
        RequestUpdateRoomPhaseOnPlayerJoinLeaveAsync(RoomModel.GetInstance().RoomName, isAdminJoin).Forget();
    }
    public void OnPlayerEquipmentConfirm(int playerId)
    {
        _playerEquipConfirmedDict[playerId] = true;
        UpdateEquipmentConfirm();
    }
    private void UpdateEquipmentConfirm()
    {
        if (_playerEquipConfirmedDict.Values.Count(x => x) >= GameConstant.GameStartPlayerCount)
        {
            if (_currentRoomPhase == RoomPhase.Waiting)
            {
                RequestUpdateRoomPhase(RoomPhase.CountDown);
                StartCountDownAdmin();
            }
        }else if (_currentRoomPhase != RoomPhase.Waiting)
        {
            RequestUpdateRoomPhase(RoomPhase.Waiting);
            CancelCountDownAdmin();
        }
    }
    private async UniTask<Unit> RequestUpdateRoomPhaseOnPlayerJoinLeaveAsync(string roomName, bool isAdminJoin = false)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if (_isSendRequestUpdateRoomPlayerCount)
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoomPlayerCount = true;
        var needGotoTitle = false;

        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            if (playerCount == 0)
            {
                needGotoTitle = true;
                await RequestUpdateRoomPhaseAsync(RoomModel.GetInstance().RoomName, RoomPhase.Waiting);
            }
            else
            {
                await RoomService.UpdateRoom(new RoomInfo(roomName, playerCount, _currentRoomPhase.ToString()), new CancellationToken());
            }

            UpdateEquipmentConfirm();
        }

        _isSendRequestUpdateRoomPlayerCount = false;

        if(needGotoTitle && !isAdminJoin)
        {
            SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Title).Forget();
        }

        return Unit.Default;
    }
    private void RequestUpdateRoomPhase(RoomPhase roomPhase)
    {
        RequestUpdateRoomPhaseAsync(RoomModel.GetInstance().RoomName, roomPhase).Forget();
    }
    private async UniTask<Unit> RequestUpdateRoomPhaseAsync(string roomName, RoomPhase roomPhase)
    {
        _requestOverrideRoomPhase = (int)roomPhase;

        if (_isSendRequestUpdateRoomPhase)
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoomPhase = true;

        while (_requestOverrideRoomPhase.HasValue)
        {
            _currentRoomPhase = (RoomPhase)_requestOverrideRoomPhase.Value;
            var playerCount = GetCurrentPlayerCount();
            _requestOverrideRoomPhase = null;

            await RoomService.UpdateRoom(new RoomInfo(roomName, playerCount, _currentRoomPhase.ToString()), new CancellationToken());
        }

        _isSendRequestUpdateRoomPhase = false;
        SyncUpdateRoomPhase();

        return Unit.Default;
    }
    private int GetCurrentPlayerCount()
    {
        return _playerInfoObjects.Keys.Count(x => x != _adminRef.PlayerId);
    }
    #region count_down
    private void StartCountDownAdmin()
    {
        var endTime = DateTime.UtcNow.AddSeconds(GameConstant.CountDownSec);
        double unixTimeMs = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();
        _roomStateController.RoomCountDownTime = unixTimeMs;
        RpcConnector.Instance.Rpc_BroadcastStartCountDown(unixTimeMs);
        UnityEngine.Debug.LogWarning($"Start CountDown Admin");
        
    }
    private void CancelCountDownAdmin()
    {
        RpcConnector.Instance.Rpc_BroadcastCancelCountDown(0);
        _roomStateController.RoomCountDownTime = 0;
    }
    #endregion
    private void SyncUpdateRoomPhase()
    {
        UnityEngine.Debug.Log($"Sync Room phase {(RoomPhase)_currentRoomPhase}");
        _roomStateController.CurrentRoomPhase = (int)_currentRoomPhase;
        RpcConnector.Instance?.Rpc_BroadcastRoomPhase(_currentRoomPhase);
    }
}

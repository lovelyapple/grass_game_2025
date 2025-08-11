using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
public interface IGameAdminModel
{
    public void OnAdminJoined(PlayerRef playerRef);
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController);
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject);
    public void OnPlayerLeave(int playerId);
    public void OnCountDownUpdate(double timeRemain);
    public void OnCountDownFinished();
    public void OnMatchStart();
    public void OnPlayerFinishedLine(int playerId);
    public void OnReturnRoomTop();
}
public class NullGameAdminModel : IGameAdminModel
{
    public void OnAdminJoined(PlayerRef playerRef) { }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController) { }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject) { }
    public void OnPlayerLeave(int playerId) { }
    public void OnCountDownUpdate(double timeRemain) { }
    public void OnMatchStart() { }
    public void OnCountDownFinished() { }
    public void OnPlayerFinishedLine(int playerId) { }
    public void OnReturnRoomTop() { }
}
public class GameAdminModel : IGameAdminModel
{
    private PlayerRef _adminRef;
    private RoomStateController _roomStateController;
    private Dictionary<int, PlayerInfoObject> _playerInfoObjects = new Dictionary<int, PlayerInfoObject>();
    private RoomPhase _currentRoomPhase;
    private bool _isSendRequestUpdateRoomPlayerCount = false;
    private bool _isSendRequestUpdateRoomPhase = false;
    private int? _playerCountRequesting = null;
    public void OnAdminJoined(PlayerRef playerRef)
    {
        _adminRef = playerRef;
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        _currentRoomPhase = RoomPhase.Waiting;
        UpdateRoomPhaseOnPlayerJoinLeave();
        SyncUpdateRoomPhase();
    }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject)
    {
        _playerInfoObjects.Add(infoObject.PlayerId, infoObject);
        UpdateRoomPhaseOnPlayerJoinLeave();
    }
    public void OnPlayerLeave(int playerId)
    {
        _playerInfoObjects.Remove(playerId);
        UpdateRoomPhaseOnPlayerJoinLeave();
    }
    private int? _requestOverrideRoomPhase = null;
    public void OnCountDownUpdate(double timeRemain)
    {
        if (timeRemain <= GameConstant.FinalCountDownSec &&
         _currentRoomPhase == RoomPhase.CountDown)
        {
            ReuestUpdateRoomPhase(RoomPhase.CountLock);
        }
    }
    public void OnCountDownFinished()
    {
        ReuestUpdateRoomPhase(RoomPhase.MatchLoading);
    }
    public void OnMatchStart()
    {
        ReuestUpdateRoomPhase(RoomPhase.Playing);
    }
    public void OnPlayerFinishedLine(int playerId)
    {
        if (MatchModel.GetInstance().MatchWinner == 0)
        {
            RpcConnector.Instance.Rpc_BroadcastMatchFinished(playerId);
            ReuestUpdateRoomPhase(RoomPhase.Result);
        }
    }
    public void OnReturnRoomTop()
    {
        ReuestUpdateRoomPhase(RoomPhase.Waiting);
    }
    private void UpdateRoomPhaseOnPlayerJoinLeave()
    {
        RequestUpdateRoomPhaseOnPlayerJoinLeaveAsync(RoomModel.GetInstance().RoomName, GetCurrentPlayerCount()).Forget();
    }
    private async UniTask<Unit> RequestUpdateRoomPhaseOnPlayerJoinLeaveAsync(string roomName, int currentPlayerCount)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if (_isSendRequestUpdateRoomPlayerCount)
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoomPlayerCount = true;

        var prevRoomPhase = _currentRoomPhase;
        var needGotoTitle = false;

        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            if (prevRoomPhase == RoomPhase.Waiting)
            {
                if (playerCount >= GameConstant.GameStartPlayerCount)
                {
                    _currentRoomPhase =  RoomPhase.CountDown;
                }
            }
            else if (prevRoomPhase == RoomPhase.CountDown || prevRoomPhase == RoomPhase.CountLock)
            {
                if (playerCount < GameConstant.GameStartPlayerCount)
                {
                    _currentRoomPhase =  RoomPhase.Waiting;
                }
            }
            else if (prevRoomPhase == RoomPhase.MatchLoading || prevRoomPhase == RoomPhase.Playing)
            {
                if(playerCount == 0)
                {
                    needGotoTitle = true;
                }
            }

            await RoomService.UpdateRoom(new RoomInfo(roomName, playerCount, _currentRoomPhase.ToString()), new CancellationToken());
        }

        _isSendRequestUpdateRoomPlayerCount = false;

        if (_currentRoomPhase != prevRoomPhase)
        {
            if (_currentRoomPhase == RoomPhase.CountDown)
            {
                StartCountDownAdmin();
            }
            else if (_currentRoomPhase == RoomPhase.Waiting)
            {
                CancelCountDownAdmin();
            }

            SyncUpdateRoomPhase();
        }

        if(needGotoTitle)
        {
            SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Title).Forget();
        }

        return Unit.Default;
    }
    private void ReuestUpdateRoomPhase(RoomPhase roomPhase)
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
        RpcConnector.Instance.Rpc_BroadcastStartCountDown(unixTimeMs);
        UnityEngine.Debug.LogWarning($"Start CountDown Admin");
    }
    private void CancelCountDownAdmin()
    {
        RpcConnector.Instance.Rpc_BroadcastCancelCountDown(0);
    }
    #endregion
    private void SyncUpdateRoomPhase()
    {
        UnityEngine.Debug.Log($"Sync Room phase {(RoomPhase)_currentRoomPhase}");
        _roomStateController.CurrentRoomPhase = (int)_currentRoomPhase;
        RpcConnector.Instance?.Rpc_BroadcastRoomPhase(_currentRoomPhase);
    }
}

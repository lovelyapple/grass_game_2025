using System;
using System.Collections.Generic;
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
}
public class NullGameAdminModel : IGameAdminModel
{
    public void OnAdminJoined(PlayerRef playerRef) { }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController) { }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject) { }
    public void OnPlayerLeave(int playerId) { }
    public void OnCountDownUpdate(double timeRemain) { }
}
public class GameAdminModel : IGameAdminModel
{
    private PlayerRef _adminRef;
    private RoomStateController _roomStateController;
    private Dictionary<int, PlayerInfoObject> _playerInfoObjects = new Dictionary<int, PlayerInfoObject>();
    private RoomPhase _currentRoomPhase;
    private bool _isSendRequestUpdateRoom = false;
    private int? _playerCountRequesting = null;
    public void OnAdminJoined(PlayerRef playerRef)
    {
        _adminRef = playerRef;
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        _currentRoomPhase = RoomPhase.Waiting;
        UpdateRoomPhase();
        SendUpdateRoomPhase();
    }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject)
    {
        _playerInfoObjects.Add(infoObject.PlayerId, infoObject);
        UpdateRoomPhase();
    }
    public void OnPlayerLeave(int playerId)
    {
        _playerInfoObjects.Remove(playerId);
        UpdateRoomPhase();
    }

    public void OnCountDownUpdate(double timeRemain)
    {
        if (timeRemain <= GameConstant.FinalCountDownSec && _currentRoomPhase == RoomPhase.CountDown)
        {
            _currentRoomPhase = RoomPhase.CountLock;
            UpdateRoomPhase();
        }
    }
    private void UpdateRoomPhase()
    {
        RequestUpdateRoomAdmin(RoomModel.GetInstance().RoomName).Forget();
    }
    private async UniTask<Unit> RequestUpdateRoomAdmin(string roomName)
    {
        _playerCountRequesting = GetCurrentPlayerCount();

        if (_isSendRequestUpdateRoom)
        {
            return Unit.Default;
        }

        _isSendRequestUpdateRoom = true;

        var prevRoomPhase = _currentRoomPhase;
        while (_playerCountRequesting.HasValue)
        {
            var playerCount = _playerCountRequesting.Value;
            _playerCountRequesting = null;

            _currentRoomPhase = GetRoomPhaseFromMember(playerCount, _currentRoomPhase);
            await RoomService.UpdateRoom(new RoomInfo(roomName, playerCount, _currentRoomPhase.ToString()), new CancellationToken());
        }

        _isSendRequestUpdateRoom = false;

        if (_currentRoomPhase != prevRoomPhase || _currentRoomPhase == RoomPhase.CountLock)
        {
            if (_currentRoomPhase == RoomPhase.CountDown)
            {
                StartCountDownAdmin();
            }
            else if (_currentRoomPhase == RoomPhase.Waiting)
            {
                CancelCountDownAdmin();
            }

            SendUpdateRoomPhase();
        }

        return Unit.Default;
    }
    private int GetCurrentPlayerCount()
    {
        return _playerInfoObjects.Keys.Count(x => x != _adminRef.PlayerId);
    }
    private RoomPhase GetRoomPhaseFromMember(int member, RoomPhase currentRoomPhase)
    {
        if (currentRoomPhase == RoomPhase.Waiting)
        {
            if (member >= GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.CountDown;
            }
        }
        else if (currentRoomPhase == RoomPhase.CountDown)
        {
            if (member < GameConstant.GameStartPlayerCount)
            {
                return RoomPhase.Waiting;
            }
        }

        return currentRoomPhase;
    }
    #region cound_down
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
    private void SendUpdateRoomPhase()
    {
        _roomStateController.CurrentRoomPhase = (int)_currentRoomPhase;
    }
}

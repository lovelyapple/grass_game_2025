using System.Collections.Generic;
using Fusion;
public interface IGameAdminModel
{
    public void OnAdminJoined(PlayerRef playerRef);
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController);
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject);
    public void OnPlayerLeave(int playerId);
}
public class NullGameAdminModel : IGameAdminModel
{
    public void OnAdminJoined(PlayerRef playerRef) { }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController) { }
    public void OnPlayerInfoObjectJoined(PlayerInfoObject infoObject) { }
    public void OnPlayerLeave(int playerId) { }
}
public class GameAdminModel : IGameAdminModel
{
    private PlayerRef _adminRef;
    private RoomStateController _roomStateController;
    private RoomPhase _roomPhase;
    private Dictionary<int, PlayerInfoObject> _playerInfoObjects = new Dictionary<int, PlayerInfoObject>();
    public void OnAdminJoined(PlayerRef playerRef)
    {
        _adminRef = playerRef;
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
        SendUpdateRoomPhase(RoomPhase.Waiting);
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
    private void UpdateRoomPhase()
    {

    }
    private void SendUpdateRoomPhase(RoomPhase roomPhase)
    {
        _roomPhase = roomPhase;
        _roomStateController.CurrentRoomPhase = (int)roomPhase;
    }
}

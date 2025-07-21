using System.Collections.Generic;
using Fusion;
public interface IGameAdminModel
{
    public void OnAdmingJoined(PlayerRef playerRef);
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController);
    public void OnPlayerInfoObjectSpawned(PlayerInfoObject infoObject);
    public void OnPlayerLeave(int playerId);
}
public class NullGameAdminModel : IGameAdminModel
{
    public void OnAdmingJoined(PlayerRef playerRef) { }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController) { }
    public void OnPlayerInfoObjectSpawned(PlayerInfoObject infoObject) { }
    public void OnPlayerLeave(int playerId) { }
}
public class GameAdminModel : IGameAdminModel
{
    private PlayerRef _adminRef;
    private RoomStateController _roomStateController;
    private RoomPhase _roomPhase;
    private Dictionary<int, PlayerInfoObject> _playerInfoObjects = new Dictionary<int, PlayerInfoObject>();
    public void OnAdmingJoined(PlayerRef playerRef)
    {
        _adminRef = playerRef;
    }
    public void OnRoomStateControllerSpawn(RoomStateController roomStateController)
    {
        _roomStateController = roomStateController;
    }
    public void OnPlayerInfoObjectSpawned(PlayerInfoObject infoObject)
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
}

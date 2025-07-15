using UnityEngine;

public enum GamePhase
{
    Initialize,
    Title,
    Ready,
    Playing,
    Finished,
}
public enum HorizontalMoveDir
{
    None,
    Left,
    Right,
}
public enum RoomPhase :byte
{
    Waiting,
    CountDown,
    Playing,
    Result,
}
public enum PlayerRole
{
    Player = 1,
    Admin = 2,
}
public static class GameConstant 
{
    public const int MaxRoomCount = 3;
    public const int MaxPlayerPerRoom = 5;
    public const int GameStartPlayerCount = 3;
}

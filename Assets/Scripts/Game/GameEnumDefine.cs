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
    CountLock,
    MatchLoading,
    Playing,
    Result,
}
public enum PlayerRole
{
    Player = 1,
    Admin = 2,
}
public enum SKillContinueType
{
    SPCost,
    OneShot,
}
public enum StatusEffectType
{
    None,
    DirectionRevert,
    Stun
}
public enum BgmType
{
    None,
    Title,
    Game,
    Result,
}
public static class GameConstant 
{
    public const int MaxRoomCount = 3;
    public const int MaxPlayerPerRoom = 5;
    public const int GameStartPlayerCount = 1;
    public const string EmptyRoomName = "Empty";
    public const int CountDownSec = 45;
    public const int FinalCountDownSec = 15;
}

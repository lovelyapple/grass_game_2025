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
public class GameEnumDefine
{
    
}

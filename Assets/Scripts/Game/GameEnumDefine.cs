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
public class GameEnumDefine
{
    
}

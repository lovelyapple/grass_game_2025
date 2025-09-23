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
public enum PlayerParameterType
{
    Hp,
    Speed,
    Heat,
}
public static class GameConstant 
{
    public const int MaxRoomCount = 3;
    public const int MaxPlayerPerRoom = 5;
    public const int GameStartPlayerCount = 1;
    public const string EmptyRoomName = "Empty";
    public const int CountDownSec = 20;
    public const int FinalCountDownSec = 15;
    public const float MaxHpBarValue = 600;
    public const float MaxHeatBarValue = 100;
    public const float MaxSpeedBarValue = 6;
    public static float GetParameterMaxValue(PlayerParameterType type)
    {
        switch(type)
        {
            case PlayerParameterType.Hp:
                return MaxHpBarValue;
            case PlayerParameterType.Speed:
                return MaxSpeedBarValue;
            default:
            case PlayerParameterType.Heat:
                return MaxHeatBarValue;
        }
    }
}

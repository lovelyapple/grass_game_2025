using UnityEngine;

public class GamePlayerInfoModel : SingletonBase<GamePlayerInfoModel>
{
    public PlayerInfoObject SelfPlayerInfoObj { get; private set; }
    public void SetSelfObject(PlayerInfoObject playerInfoObject)
    {
        SelfPlayerInfoObj = playerInfoObject;
    }
}

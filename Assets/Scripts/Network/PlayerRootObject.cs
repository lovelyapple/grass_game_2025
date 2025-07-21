using System.Collections.Generic;
using Fusion;
using UnityEngine;
using R3;

public class PlayerRootObject : MonoBehaviour
{
    private static PlayerRootObject _intance;
    public static PlayerRootObject Instance
    {
        get
        {
            if (_intance == null)
            {
                _intance = new GameObject("PlayerRootObject").AddComponent<PlayerRootObject>();
            }

            return _intance;
        }
    }
    public Dictionary<int, PlayerInfoObject> PlayerInfos = new Dictionary<int, PlayerInfoObject>();
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(x => OnPlayerLeave(x))
        .AddTo(this);
    }
    public void OnPlayerInfoSpawned(PlayerInfoObject playerInfoObj)
    {
        PlayerInfos.Add(playerInfoObj.PlayerRef.PlayerId, playerInfoObj);
        playerInfoObj.transform.parent = transform;
        playerInfoObj.gameObject.name = $"PlayerInfoObject_{playerInfoObj.PlayerRef.PlayerId}";
    }
    public void OnPlayerLeave(int playerId)
    {
        var obj = PlayerInfos[playerId];
        PlayerInfos.Remove(playerId);
        Destroy(obj.gameObject);
    }
}

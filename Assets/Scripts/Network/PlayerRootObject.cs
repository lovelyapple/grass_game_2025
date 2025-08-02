using System.Collections.Generic;
using Fusion;
using UnityEngine;
using R3;
using StarMessage.Models;

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
    public PlayerInfoObject SelfInfoObject = null;
    public Dictionary<int, PlayerInfoObject> PlayerInfos = new Dictionary<int, PlayerInfoObject>();
    // obj がちゃんと初期化されるまでに、leaveした場合の保険
    private List<int> _requestLeavePlayerIds = new List<int>();

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(x => OnPlayerLeave(x))
        .AddTo(this);
    }
    public void OnPlayerInfoSpawnedAndRegister(PlayerInfoObject playerInfoObj)
    {
        if(_requestLeavePlayerIds.Contains(playerInfoObj.PlayerRef.PlayerId))
        {
            Destroy(playerInfoObj.gameObject);
            return;
        }

        PlayerInfos.Add(playerInfoObj.PlayerRef.PlayerId, playerInfoObj);
        playerInfoObj.transform.parent = transform;
        playerInfoObj.gameObject.name = $"PlayerInfoObject_{playerInfoObj.PlayerRef.PlayerId}";
        RoomModel.GetInstance().OnPlayerInfoObjectJoined(playerInfoObj);
    }
    public void OnPlayerLeave(int playerId)
    {
        if(!PlayerInfos.ContainsKey(playerId))
        {
            _requestLeavePlayerIds.Add(playerId);
            return;
        }

        var obj = PlayerInfos[playerId];
        PlayerInfos.Remove(playerId);
        
        if(GameCoreModel.Instance.IsAdminUser)
            Destroy(obj.gameObject);
        ModelCache.Admin.OnPlayerLeave(playerId);
    }
    public string GetPlayerInfoName(int playerId)
    {
        if(PlayerInfos.ContainsKey(playerId))
        {
            if(PlayerInfos[playerId].PlayerId <= 0)
            {
                return null;
            }

            return PlayerInfos[playerId].PlayerName;
        }

        return null;
    }
    public PlayerInfoObject GetPlayerInfoObject(int playerId)
    {
        if (PlayerInfos.TryGetValue(playerId, out var obj))
        {
            return obj;
        }

        Debug.LogError($"PlayerInfoobj is null {playerId}");
        return null;
    }
}

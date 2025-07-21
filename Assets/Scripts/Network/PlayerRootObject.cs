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
    // obj がちゃんと初期化されるまでに、leaveした場合の保険
    private List<int> _requestLeavePlayerIds = new List<int>();

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        RoomModel.GetInstance().OnPlayerLeaveObservable()
        .Subscribe(x => OnPlayerLeave(x))
        .AddTo(this);
    }
    public void OnPlayerInfoSpawned(PlayerInfoObject playerInfoObj)
    {
        if(_requestLeavePlayerIds.Contains(playerInfoObj.PlayerRef.PlayerId))
        {
            Destroy(playerInfoObj.gameObject);
            return;
        }

        PlayerInfos.Add(playerInfoObj.PlayerRef.PlayerId, playerInfoObj);
        playerInfoObj.transform.parent = transform;
        playerInfoObj.gameObject.name = $"PlayerInfoObject_{playerInfoObj.PlayerRef.PlayerId}";
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
        Destroy(obj.gameObject);
    }
    public string GetPlayerInfoName(int playerId)
    {
        if(PlayerInfos.ContainsKey(playerId))
        {
            if(PlayerInfos[playerId].BaseInfoStruct.PlayerId <= 0)
            {
                return null;
            }

            return PlayerInfos[playerId].BaseInfoStruct.PlayerName;
        }

        return null;
    }
}

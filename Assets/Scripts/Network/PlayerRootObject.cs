using System.Collections.Generic;
using Fusion;
using UnityEngine;
using R3;
using StarMessage.Models;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Collections;
using System.Linq;

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
    public void Reset()
    {
        _requestLeavePlayerIds.Clear();
        foreach(var obj in PlayerInfos.Values)
        {
            if(obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        PlayerInfos.Clear();
        SelfInfoObject = null;
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
        Debug.LogWarning($"OnPlayerInfoSpawnedAndRegister {playerInfoObj.PlayerRef.PlayerId}, name {playerInfoObj.name}");
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
        if (PlayerInfos.ContainsKey(playerId))
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
    public async UniTask<Unit> WaitAllObjectReadyAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => SelfInfoObject != null && !string.IsNullOrEmpty(SelfInfoObject.PlayerName), cancellationToken: token);
        await UniTask.WaitUntil(() => PlayerInfos.Values.All(x => x.PlayerId != RoomModel.GetInstance().AdminId && !string.IsNullOrEmpty(x.PlayerName)), cancellationToken: token);

        return Unit.Default;
    }
    public async UniTask<Unit> WaitAllObjectMatchPreReadyAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => SelfInfoObject != null && !string.IsNullOrEmpty(SelfInfoObject.PlayerName), cancellationToken: token);
        await UniTask.WaitUntil(() => PlayerInfos.Values.All(x => x.PlayerId != RoomModel.GetInstance().AdminId && x.IsMatchPreReady), cancellationToken: token);

        return Unit.Default;
    }
}

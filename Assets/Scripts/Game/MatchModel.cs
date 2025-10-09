using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;

public class MatchModel :SingletonBase<MatchModel>
{
    public MatchPlayerModel SelfPlayer { get; private set; }
    private List<MatchPlayerModel> _players = new List<MatchPlayerModel>();
    public List<MatchPlayerModel> Players => _players;
    private Subject<bool> _showLoadUISubject = new Subject<bool>();
    public Observable<bool> ShowLoadUIObservable() => _showLoadUISubject;
    private Subject<MatchPlayerModel> _onPlayerCtrlSpawned = new Subject<MatchPlayerModel>();
    public Observable<MatchPlayerModel> OnPlayerCtrlSpawnedObservable() => _onPlayerCtrlSpawned;
    private readonly Subject<int> _onmatchFinishedSubject = new Subject<int>();
    public Observable<int> OnMatchFinishedObservable() => _onmatchFinishedSubject;
    private Subject<SpecialPoint> _specialPointChangeSubject = new Subject<SpecialPoint>();
    public Observable<SpecialPoint> SpecialPointChangeObservable() => _specialPointChangeSubject;
    private Subject<HealthPoint> _healthPointChangeSubject = new Subject<HealthPoint>();
    public Observable<HealthPoint> HealthPointChangeObservable() => _healthPointChangeSubject;
    private Subject<int> _onAnyOneUseSkillSubject = new Subject<int>();
    public Observable<int> OnAnyOneUseSkillObservable() => _onAnyOneUseSkillSubject;
    private bool _preInitFinished = false;
    public int InitializedPlayerCount { get; private set; }
    public int MatchWinner { get; private set; }
    public float RaceDistance { get; set; }
    private Dictionary<int, FieldItemBase> _itemDict = null;
    public void Reset()
    {
        SelfPlayer = null;
        _players.Clear();
        MatchWinner = 0;
        InitializedPlayerCount = 0;
        _preInitFinished = false;
        _itemDict = null;
    }
    public void OnPlayerLeave(int playerId)
    {
        _players.RemoveAll(x => x.PlayerId == playerId);

        // まぁ別になくてもいいけど
        if(SelfPlayer != null && SelfPlayer.PlayerId == playerId)
        {
            SelfPlayer = null;
        }

        MatchWinner = 0;
    }
    public async UniTaskVoid RequestStartMatchAsync(CancellationToken token)
    {
        Debug.LogWarning("@@@ RequestStartMatchAsync 1");
        await SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Game);
        _showLoadUISubject.OnNext(true);

        SoundManager.PlayBgm(bgmType: BgmType.Game);

        var playerObjs = PlayerRootObject.Instance.PlayerInfos.Values;

        foreach (var info in playerObjs)
        {
            var model = new MatchPlayerModel();
            model.Initialize(info);
            _players.Add(model);
            Debug.Log($"RequestStartMatchAsync add player {model.PlayerId}");
            if (info.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
            {
                SelfPlayer = model;
            }
        }

        _players = _players.OrderBy(x => x.PlayerId)
        .Select((x, index) =>
        {
            x.SetupIndex(index);
            return x;
        })
        .ToList();

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            PlayerRootObject.Instance.SelfInfoObject.IsMatchPreReady = true;
        }

        Debug.LogWarning("@@@ RequestStartMatchAsync 2");
        await PlayerRootObject.Instance.WaitAllObjectMatchPreReadyAsync(token);

        InitializedPlayerCount = _players.Count;
        _preInitFinished = true;

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.LoadCtrlPlayerSelf();
        }

        // ロード途中に抜けると死ぬ
        async UniTask WaitUntilReady(MatchPlayerModel player)
        {
            await UniTask.WaitUntil(() => player.IsResourceReady, cancellationToken: token);
        }

        var tasks = _players.Select(player => WaitUntilReady(player)).ToArray();
        Debug.LogWarning("@@@ RequestStartMatchAsync 3");
        await UniTask.WhenAll(tasks);

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.FieldPlayerController.RegistInput();
            MatchCameraController.Instance.SetupTarget(SelfPlayer.FieldPlayerController.transform);
        }

        ModelCache.Admin.OnMatchStart();

        Debug.LogWarning("@@@ RequestStartMatchAsync 4");
        await UniTask.WaitUntil(() => 
        RoomStateController.Instance == null ||
        RoomStateController.Instance.CurrentRoomPhase == (int)RoomPhase.Playing,
        cancellationToken: token);

        _showLoadUISubject.OnNext(false);
    }
    public void OnFieldPlayerControllerSpawned(FieldPlayerController fieldPlayerController)
    {
        SetFieldPlayerControllerAsync(fieldPlayerController).Forget();
    }
    private async UniTask<Unit> SetFieldPlayerControllerAsync(FieldPlayerController fieldPlayerController)
    {
        await UniTask.WaitUntil(() => _preInitFinished && fieldPlayerController != null);

        var playerModel = GetPlayer(fieldPlayerController.PlayerId);
        playerModel.OnFieldPlayerControllerSpawned(fieldPlayerController);
        _onPlayerCtrlSpawned.OnNext(playerModel);

        return Unit.Default;
    }
    public void SetMatchResult(int playerId)
    {
        MatchWinner = playerId;
        _onmatchFinishedSubject.OnNext(MatchWinner);

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.FieldPlayerController.ReleaseController();
        }

        Debug.Log($"Match has winner {playerId}");
    }
    public void ReceivedPlayerJumpInOut(int playerId, bool jumpIn)
    {
        var model = GetPlayer(playerId);

        if(model != null)
        {
            model.GetModelObservable().DoAsync(x => x.OnReceivedJumpInOut(jumpIn)).Forget();
        }
    }
    public void ReceivedPlayerUseSkill(int playerId)
    {
        var model = GetPlayer(playerId);

        if (model != null)
        {
            model.GetModelObservable().DoAsync(x =>
            {
                x.OnReceivedUseSkill();
                if(x.SkillBase is SkillJK)
                {
                    _onAnyOneUseSkillSubject.OnNext((int)Characters.JK);
                }
                else if(x.SkillBase is SkillOfficeWorker)
                {
                    _onAnyOneUseSkillSubject.OnNext((int)Characters.OfficeWorker);
                }
                else
                {
                    _onAnyOneUseSkillSubject.OnNext((int)Characters.Sumo);
                }
            }).Forget();
        }
    }
    public void ReceivedPlayeFinishSkill(int playerId)
    {
        var model = GetPlayer(playerId);

        if (model != null)
        {
            model.GetModelObservable().DoAsync(x => x.OnReceivedFinishSkill()).Forget();
        }
    }
    public void ReceivedRequestTouchStatusEffect(int playerId, int effectType)
    {
        var model = GetPlayer(playerId);

        if (model != null)
        {
            model.GetModelObservable().DoAsync(x => x.OnReceivedStatusEffect(effectType, true)).Forget();
        }
    }
    public void UpdateHeatAndSepcialPoint(SpecialPoint specialPoint, HealthPoint healthPoint)
    {
        _specialPointChangeSubject.OnNext(specialPoint);
        _healthPointChangeSubject.OnNext(healthPoint);
    }
    public void OnSelfUseStatusEffectSkill(int effectType)
    {
        var otherPlayers = _players.Where(x => x.PlayerId != SelfPlayer.PlayerId).ToList();

        foreach(var player in otherPlayers)
        {
            RpcConnector.Instance.Rpc_BroadcastOnRequestTouchPlayerStatusEffect(player.PlayerId, effectType);
        }
    }
    private MatchPlayerModel GetPlayer(int playerId)
    {
        var player = _players.FirstOrDefault(x => x.PlayerId == playerId);

        if(player == null)
        {
            Debug.LogError($"MatchPlayerModel not found {playerId}");
        }

        return player;
    }
    public float TranslatePlayerProgress(float zPosition)
    {
        return zPosition / (RaceDistance + 0.01f);
    }
    private FieldItemBase TryGetItem(int itemBoxId)
    {
        if (_itemDict == null)
        {
            _itemDict = new Dictionary<int, FieldItemBase>();

            var allItems = GameObject.FindObjectsByType<FieldItemBase>(FindObjectsSortMode.InstanceID);

            for (int i = 0; i < allItems.Length; i++)
            {
                _itemDict.Add(allItems[i].ItemId, allItems[i]);
            }
        }

        if(_itemDict.TryGetValue(itemBoxId, out var item))
        {
            return item;
        }

        Debug.LogError($"itemBoxId not found {itemBoxId}");
        return null;
    }
    public FieldItemBase TryOpenItemAdmin(int itemBoxId)
    {
        var item = TryGetItem(itemBoxId);

        if(item == null)
        {
            return null;
        }

        if (item.Used)
        {
            return null;
        }

        item.OnUse();
        return item;
    }
    public void TrySetItemActive(int itemBoxId, bool active)
    {
        var item = TryGetItem(itemBoxId);

        if(item == null)
        {
            return;
        }

        item.SetActive(active);
    }
    public void ReceivedItemBoxEffect(int playerId, int itemEffectType)
    {
        var model = GetPlayer(playerId);

        if (model != null)
        {
            model.GetModelObservable().DoAsync(x => x.OnReceivedItemEffect(itemEffectType)).Forget();
        }
    }
}

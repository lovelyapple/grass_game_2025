using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
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

    public void Reset()
    {
        SelfPlayer = null;
        _players.Clear();
        MatchWinner = 0;
        InitializedPlayerCount = 0;
        _preInitFinished = false;
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
        await SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Game);
        _showLoadUISubject.OnNext(true);

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

        InitializedPlayerCount = _players.Count;
        _preInitFinished = true;

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.LoadCtrlPlayerSelf();
        }

        // ロード途中に抜けると死ぬ
        async UniTask WaitUntilReady(MatchPlayerModel player)
        {
            await UniTask.WaitUntil(() => player.IsResourceReady);
        }

        var tasks = _players.Select(player => WaitUntilReady(player)).ToArray();
        await UniTask.WhenAll(tasks);

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.FieldPlayerController.RegistInput();
            MatchCameraController.Instance.SetupTarget(SelfPlayer.FieldPlayerController.transform);
        }

        ModelCache.Admin.OnMatchStart();

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
            model.GetModelObservable().DoAsync(x => x.OnReceivedStatusEffect(effectType)).Forget();
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
}

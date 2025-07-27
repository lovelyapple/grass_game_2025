using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using StarMessage.Models;
using UnityEngine;

public class MatchModel :SingletonBase<MatchModel>
{
    public MatchPlayerModel SelfPlayer { get; private set; }
    private List<MatchPlayerModel> _players = new List<MatchPlayerModel>();
    private Subject<bool> _showLoadUISubject = new Subject<bool>();
    public Observable<bool> ShowLoadUIObservable() => _showLoadUISubject;
    private Subject<MatchPlayerModel> _onPlayerCtrlSpawned = new Subject<MatchPlayerModel>();
    public Observable<MatchPlayerModel> OnPlayerCtrlSpawnedObservable() => _onPlayerCtrlSpawned;
    public int InitializedPlayerCount { get; private set; }
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

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            SelfPlayer.LoadCtrlPlayerSelf();
        }

        async UniTask WaitUntilReady(MatchPlayerModel player)
        {
            await UniTask.WaitUntil(() => player.IsResourceReady);
        }

        var tasks = _players.Select(player => WaitUntilReady(player)).ToArray();
        await UniTask.WhenAll(tasks);

        _showLoadUISubject.OnNext(false);
    }
    public void OnFieldPlayerControllerSpawned(FieldPlayerController fieldPlayerController)
    {
        var playerModel = _players.FirstOrDefault(x => x.PlayerId == fieldPlayerController.PlayerId);
        playerModel.OnFieldPlayerControllerSpawned(fieldPlayerController);
        _onPlayerCtrlSpawned.OnNext(playerModel);
    }
    private void Reset()
    {

    }
}

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MatchModel :SingletonBase<MatchModel>
{
    private MatchPlayerModel SelfPlayer = null;
    private List<MatchPlayerModel> _players = new List<MatchPlayerModel>();
    private async UniTaskVoid RequestStartMatchAsync()
    {
        var playerObjs = PlayerRootObject.Instance.PlayerInfos.Values;

        foreach(var info in playerObjs)
        {
            var model = new MatchPlayerModel();
            model.Initialize(info);
            _players.Add(model);

            if(info.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
            {
                SelfPlayer = model;
            }
        }

        SelfPlayer.LoadCtrlPlayerSelf();

        async UniTask WaitUntilReady(MatchPlayerModel player)
        {
            await UniTask.WaitUntil(() => player.IsResourceReady);
        }

        var tasks = _players.Select(player => WaitUntilReady(player)).ToArray();
        await UniTask.WhenAll(tasks);
    }
    public void OnFieldPlayerControllerSpawned(FieldPlayerController fieldPlayerController)
    {
        var playerModel = _players.FirstOrDefault(x => x.PlayerId == fieldPlayerController.PlayerId);
        playerModel.OnFieldPlayerControllerSpawned(fieldPlayerController);
    }
    private void Reset()
    {

    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

public class MatchController : MonoBehaviour
{
    [SerializeField] FieldStartLineObject StartLineObject;
    [SerializeField] Transform EndLineObject;
    private void Awake()
    {
        MatchModel.GetInstance().OnPlayerCtrlSpawnedObservable()
        .Subscribe(player => OnPlayerSpawned(player))
        .AddTo(this);
    }  
    private void OnPlayerSpawned(MatchPlayerModel player)
    {
        if (player.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            var playerCount = MatchModel.GetInstance().InitializedPlayerCount;
            var pos = StartLineObject.GetStartPoint(playerCount, player.PlayerIndex);

            player.FieldPlayerController.SetupInitPos(pos);

            player.FieldPlayerController.OnZPosUpdatedObservable()
            .Subscribe(ctrl => OnSelfPlayerMove(ctrl))
            .AddTo(this);
        }
    }
    private void OnSelfPlayerMove(FieldPlayerController ctrl)
    {
        if(ctrl.transform.position.z >= EndLineObject.transform.position.z && !ctrl.IsFinished)
        {
            ctrl.IsFinished = true;
            RpcConnector.Instance.Rpc_BroadcastFinishLine(ctrl.PlayerId);
        }
    }
} 

using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

public class MatchController : MonoBehaviour
{
    [SerializeField] FieldStartLineObject StartLineObject;
    private void Awake()
    {
        MatchModel.GetInstance().OnPlayerCtrlSpawnedObservable()
        .Subscribe(player => SetPlayerPosition(player))
        .AddTo(this);
    }  
    private void SetPlayerPosition(MatchPlayerModel player)
    {
        if (player.PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            var playerCount = MatchModel.GetInstance().InitializedPlayerCount;
            var pos = StartLineObject.GetStartPoint(playerCount, player.PlayerIndex);

            player.FieldPlayerController.SetupInitPos(pos);

        }
    }
} 

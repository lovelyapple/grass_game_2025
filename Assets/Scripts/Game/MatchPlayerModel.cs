using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class MatchPlayerModel
{
    public int PlayerId { get; private set; }
    public FieldPlayerController FieldPlayerController { get; private set; }
    private EquipmentSetInfo _equipmentInfo;
    public bool IsResourceReady => FieldPlayerController != null && FieldPlayerController.IsReady;
    public int PlayerIndex{ get; private set; }
    readonly Subject<MatchPlayerModel> _onReady = new Subject<MatchPlayerModel>();
    private Observable<MatchPlayerModel> ReadyAsObservable() => _onReady;
    public void Initialize(PlayerInfoObject obj)
    {
        PlayerId = obj.PlayerId;
        _equipmentInfo = new EquipmentSetInfo(obj.PlayerEquipment);
    }
    public void SetupIndex(int index)
    {
        PlayerIndex = index;
    }
    public void LoadCtrlPlayerSelf()
    {
        if (PlayerId == RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            ShareSpawner.Instance.LoadVehicle(_equipmentInfo.Vehicle);
        }
    }
    public void OnFieldPlayerControllerSpawned(FieldPlayerController controller)
    {
        FieldPlayerController = controller;
        ToReadyAsync().Forget();
    }

    private async UniTask<Unit> ToReadyAsync()
    {
        await UniTask.WaitUntil(() => FieldPlayerController.IsReady || FieldPlayerController == null);

        if(FieldPlayerController == null)
        {
            Debug.LogError($"PlayerModel Ready Cancelled {PlayerId}");
        }

        _onReady.OnNext(this);
        _onReady.OnCompleted();

        return Unit.Default;
    }
    public Observable<FieldPlayerController> GetModelObservable() => ReadyAsObservable().Where(x => x.FieldPlayerController != null).Select(x => x.FieldPlayerController);
}

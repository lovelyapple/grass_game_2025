using Cysharp.Threading.Tasks;
using R3;
using StarMessage.Models;
using UnityEngine;
public class FieldBarrer : FieldItemBase
{
    [SerializeField] GameObject NormalImage;
    [SerializeField] GameObject BrokeImage;
    [SerializeField] Collider Collider;
    private const float RespawnSec = 5;
    public override void OnUse()
    {
        Used = true;
        SetActive(false);
        RunRespawnAsync().Forget();
    }
    private async UniTask<Unit> RunRespawnAsync()
    {
        await UniTask.WaitForSeconds(RespawnSec, cancellationToken: destroyCancellationToken);
        Used = false;
        SetActive(true);

        ModelCache.Admin.OnItemCoolDownFinished(ItemId);
        return Unit.Default;
    }
    public override void SetActive(bool active)
    {
        NormalImage.SetActive(active);
        BrokeImage.SetActive(!active);
        Collider.enabled = active;
    }
}

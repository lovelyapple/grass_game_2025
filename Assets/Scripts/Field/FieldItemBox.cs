using Cysharp.Threading.Tasks;
using R3;
using StarMessage.Models;
public enum ItemEffectType
{
    Heal,
    SpeedUp,
    BuffMax,
    Jummer,
}

public class FieldItemBox : FieldItemBase
{
    private const float RespawnSec = 5;
    public override bool IsBuffItem => true;
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
        gameObject.SetActive(true);

        ModelCache.Admin.OnItemCoolDownFinished(ItemId);
        return Unit.Default;
    }
    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}

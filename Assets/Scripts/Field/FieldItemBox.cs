using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using StarMessage.Models;
using UnityEngine;
public enum ItemEffectType
{
    Heal,
    SpeedUp,
    BuffMax,
    Jummer,
}
public interface ItemEffectBase
{
    public bool IsFieldJummer { get; }
    public ItemEffectType EffectType { get; }
}
public class HPRecoverItem : ItemEffectBase
{
    public bool IsFieldJummer => false;
    public ItemEffectType EffectType => ItemEffectType.Heal;
}
public class SpeedUpItem : ItemEffectBase
{
    public bool IsFieldJummer => false;
    public ItemEffectType EffectType => ItemEffectType.SpeedUp;
}
public class FieldItemBox : NetworkBehaviour
{
    public int ItemId;
    public bool Used;
    [ContextMenu("RematchId")]
    public void RematchAllId()
    {
        var allItems = FindObjectsByType<FieldItemBox>(FindObjectsSortMode.InstanceID);

        for(int i = 0; i < allItems.Length; i++)
        {
            allItems[i].ItemId = i;
        }
    }
    private const float RespawnSec = 5;
    public void OnUse()
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
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}

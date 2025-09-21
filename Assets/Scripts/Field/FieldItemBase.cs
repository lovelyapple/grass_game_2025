using Fusion;
using UnityEngine;

public class FieldItemBase : NetworkBehaviour
{
    public int ItemId;
    public bool Used;
    public virtual bool IsBuffItem => false;
    [ContextMenu("RematchId")]
    public void RematchAllId()
    {
        var allItems = FindObjectsByType<FieldItemBase>(FindObjectsSortMode.InstanceID);

        for (int i = 0; i < allItems.Length; i++)
        {
            allItems[i].ItemId = i;
        }
    }
    public virtual void OnUse()
    {

    }
    public virtual void SetActive(bool active)
    {

    }
}

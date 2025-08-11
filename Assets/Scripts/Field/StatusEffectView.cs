using UnityEngine;

public class StatusEffectView : MonoBehaviour
{
    [SerializeField] SpriteRenderer IconRender;
    public void SetImage(StatusEffectType statusEffectType)
    {
        IconRender.gameObject.SetActive(true);
        IconRender.sprite = ResourceContainer.Instance.GetStatusEffectIcon(statusEffectType);
    }
    public void TurnOff()
    {
        IconRender?.gameObject.SetActive(false);
    }
}

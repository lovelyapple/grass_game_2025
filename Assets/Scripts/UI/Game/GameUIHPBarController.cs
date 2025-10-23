using UnityEngine;
using UnityEngine.UI;

public class GameUIHPBarController : MonoBehaviour
{
    [SerializeField] Image RootImage;
    [SerializeField] Image HPValueImage;
    [SerializeField] Animator AlertAnimator;
    private float _maxHp = 0;
    public void UpdateCurrentHp(HealthPoint healthPoint)
    {
        _maxHp = healthPoint.TotalPoint;
        RootImage.fillAmount = _maxHp / GameConstant.MaxHpBarValue;
        HPValueImage.fillAmount = healthPoint.CurrentPoint / GameConstant.MaxHpBarValue;
    }
    public void PlayAlert(bool play)
    {
        AlertAnimator.enabled = play;

        if (!play)
        {
            var color = HPValueImage.color;
            color.a = 1;
            HPValueImage.color = color;
        }
    }
}

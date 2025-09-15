using UnityEngine;
using UnityEngine.UI;

public class GameUIHPBarController : MonoBehaviour
{
    [SerializeField] Image RootImage;
    [SerializeField] Image HPValueImage;
    private float _maxHp = 0;
    public void UpdateCurrentHp(HealthPoint healthPoint)
    {
        _maxHp = healthPoint.TotalPoint;
        RootImage.fillAmount = _maxHp / GameConstant.MaxHpBarValue;
        HPValueImage.fillAmount = healthPoint.CurrentPoint / GameConstant.MaxHpBarValue;
    }
}

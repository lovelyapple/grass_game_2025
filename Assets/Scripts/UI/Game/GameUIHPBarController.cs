using UnityEngine;
using UnityEngine.UI;

public class GameUIHPBarController : MonoBehaviour
{
    [SerializeField] Image RootImage;
    [SerializeField] Image HPValueImage;
    private const float BarMaxValue = 2000;
    private float _maxHp = 0;
    public void UpdateCurrentHp(HealthPoint healthPoint)
    {
        _maxHp = healthPoint.TotalPoint;
        RootImage.fillAmount = _maxHp / BarMaxValue;
        HPValueImage.fillAmount = _maxHp / BarMaxValue;
    }
}

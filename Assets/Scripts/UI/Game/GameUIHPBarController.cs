using UnityEngine;
using UnityEngine.UI;

public class GameUIHPBarController : MonoBehaviour
{
    [SerializeField] Image RootImage;
    [SerializeField] Image HPValueImage;
    private const float BarMaxValue = 800;
    private float _maxHp = 0;
    public void UpdateCurrentHp(HealthPoint healthPoint)
    {
        _maxHp = healthPoint.TotalPoint;
        RootImage.fillAmount = _maxHp / BarMaxValue;
        HPValueImage.fillAmount = healthPoint.CurrentPoint / BarMaxValue;
    }
}

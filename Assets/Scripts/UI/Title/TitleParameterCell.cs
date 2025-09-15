using UnityEngine;
using UnityEngine.UI;

public class TitleParameterCell : MonoBehaviour
{
    [SerializeField] PlayerParameterType Type;
    [SerializeField] Image ValueSprite;
    public float MaxValue;
    public void UpdateValue(float value)
    {
        MaxValue = GameConstant.GetParameterMaxValue(Type);
        ValueSprite.fillAmount = value / MaxValue;
    }
}

using UnityEngine;
public interface ItemEffectBase
{
    public bool IsFieldJummer { get; }
}
public class HPRecoverItem : ItemEffectBase
{
    public bool IsFieldJummer => false;
}
public class FieldItemBox : MonoBehaviour
{

}

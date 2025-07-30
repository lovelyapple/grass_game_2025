using UnityEngine;

public class DebugMatchController : MonoBehaviour
{
    public static DebugMatchController Instance;
    public void Awake()
    {
        Instance = this;
    }
    
}

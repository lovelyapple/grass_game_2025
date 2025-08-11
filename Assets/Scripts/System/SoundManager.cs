using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private SoundManager _instance;
    public void Awake()
    {
        _instance = this;
    }
}

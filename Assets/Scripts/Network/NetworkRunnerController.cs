using Fusion;
using UnityEngine;

public class NetworkRunnerController : MonoBehaviour
{
    private NetworkRunner _runner;
    private static NetworkRunnerController _instance;
    private void Awake()
    {
        _instance = this;
    }
    public static NetworkRunner Runner
    {
        get
        {
            if(_instance._runner == null)
            {
                _instance._runner = _instance.gameObject.GetComponent<NetworkRunner>();
            }

            return _instance._runner;
        }
    }
}

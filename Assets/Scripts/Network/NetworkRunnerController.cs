using Fusion;
using UnityEngine;

public class NetworkRunnerController : MonoBehaviour
{
    [SerializeField] GameObject NetworkRunnerPrefab;
    private NetworkRunner _runner;
    private static NetworkRunnerController _instance;
    private void Awake()
    {
        if (_instance != null)
        {
            _instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        var runner = Runner;
    }
    public static NetworkRunner Runner
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<NetworkRunnerController>();
            }

            if (_instance._runner == null)
            {
                var hierachyObj = GameObject.FindFirstObjectByType<NetworkRunner>();

                if (hierachyObj == null)
                {
                    hierachyObj = GameObject.Instantiate(_instance.NetworkRunnerPrefab).GetComponent<NetworkRunner>();
                }

                DontDestroyOnLoad(hierachyObj.gameObject);

                _instance._runner = hierachyObj;

                var trackter = FindFirstObjectByType<RoomPlayerTracker>();
                _instance._runner.AddCallbacks(trackter);
            }

            return _instance._runner;
        }
    }
}

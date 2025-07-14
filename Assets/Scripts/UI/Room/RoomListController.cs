using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class RoomListController : MonoBehaviour
{
    [SerializeField] GameObject RoomPrefab;
    [SerializeField] Transform RoomListRootTransform;
    [SerializeField] Button RefreshButton;
    [SerializeField] Button CloseButton;
    public void Awake()
    {

    }

    private async UniTask<Unit> RefreshRoomListAsync(CancellationToken token)
    {
        await 
    }
}

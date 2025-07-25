using System;
using System.Collections.Generic;
using System.Linq;
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
    private List<RoomCell> _roomCells = new List<RoomCell>();
    private CancellationTokenSource _refreshCancellationTokenSource;
    private bool _isRefreshing = false;
    private readonly List<IDisposable> _subscriptions = new();
    private string _roonName = null;
    public void Awake()
    {
        RefreshButton.OnClickAsObservable()
        .Where(_ => !_isRefreshing)
        .Subscribe(async async  => await RefreshRoomListAsync())
        .AddTo(this);

        CloseButton.OnClickAsObservable()
        .Subscribe(_ =>
        {
            gameObject.SetActive(false);
            _roonName = GameConstant.EmptyRoomName;
        })
        .AddTo(this);
    }
    private void OnDisable()
    {
        _refreshCancellationTokenSource?.Cancel();
        _refreshCancellationTokenSource = null;
    }
    private void OnDestory()
    {
        _subscriptions.ForEach(d => d.Dispose());
    }
    public async UniTask<string> BeginSelectRoomAsync(CancellationToken token)
    {
        _roonName = null;
        gameObject.SetActive(true);
        await RefreshRoomListAsync();

        await UniTask.WaitUntil(() => !string.IsNullOrEmpty(_roonName), cancellationToken: token);
        gameObject.SetActive(false);
        return _roonName;
    }
    private async UniTask<Unit> RefreshRoomListAsync()
    {
        _isRefreshing = true;
        RefreshButton.interactable = false;

        _refreshCancellationTokenSource = new CancellationTokenSource();
        _refreshCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, _refreshCancellationTokenSource.Token);

        try
        {
            var rooms = await RoomService.FetchRoomList(_refreshCancellationTokenSource.Token);

            foreach(var room in rooms)
            {
                var cell = _roomCells.FirstOrDefault(x => x.RoomName == room.room_name);

                if(cell == null)
                {
                    cell = Instantiate(RoomPrefab, RoomListRootTransform).GetComponent<RoomCell>();
                    cell.Initialize(room.room_name, room.player_count, room.status);
                    var diposable = cell.OnClickObservable
                    .Subscribe(roomName =>
                    {
                        _roonName = roomName;
                    });

                    _subscriptions.Add(diposable);
                    _roomCells.Add(cell);
                }
                else
                {
                    cell.UpdateCell(room.player_count, room.status);
                }
            }
        }
        finally
        {
            _isRefreshing = false;
            RefreshButton.interactable = true;
        }

        return Unit.Default;
    }
}

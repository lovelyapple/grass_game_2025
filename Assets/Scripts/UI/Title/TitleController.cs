using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using System.Threading;
using StarMessage.Models;
using Fusion;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;
using System.Threading.Tasks;
public class TitleController : MonoBehaviour
{
    [SerializeField] JoinPlayerTypeSelectView JoinSelectView;
    [SerializeField] RoomListController RoomListController;
    [SerializeField] RoomReadyController RoomReadyController;
    public void Start()
    {
        InitializeAsync().Forget();
    }
    private async UniTask<Unit> InitializeAsync()
    {
        // network事前準備
        NetworkRunnerController.RefreshLocal();
        SoundManager.PlayBgm(bgmType: BgmType.Title);
        var token = this.destroyCancellationToken;

        var adminInstance = ModelCache.Admin;
        var hasConnecting = RoomStateController.Instance;

        var roomName = "";
        var isAdmin = false;

        // 未選択の状態
        if (adminInstance == null)
        {
            var playRoleAndroomName = await JoinSelectView.OpenViewAsync(token);
            isAdmin = playRoleAndroomName.Item1 == PlayerRole.Admin;
            roomName = playRoleAndroomName.Item2;

            GameCoreModel.Instance.IsAdminUser = isAdmin;
        }
        // なんかしらの理由でタイトルに戻った
        else
        {
            isAdmin = adminInstance.IsAdmin;
        }

        SoundManager.RequestLowSmoothPGM(token);

        if(isAdmin)
        {
            // 起動後初めてAdminとして選択した
            if (!hasConnecting)
            {
                ModelCache.GetInstance().LoadAdminAs(true);
                var runner = NetworkRunnerController.Runner;

                var startResult = await runner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                    SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                    PlayerCount = GameConstant.MaxPlayerPerRoom,
                });

                if (!startResult.Ok)
                {
                    var boolResult = false;
                    CommonDialogController.Show($"admin start srever error {startResult.ErrorMessage} - {startResult.ShutdownReason}", onConfirm: () =>
                    {
                        boolResult = true;
                    });

                    await UniTask.WaitUntil(() => boolResult, cancellationToken: token);
                    return Unit.Default;
                }
            }
            else
            {
                // 必要な情報をリセットするまで待ち、RoomStateを戻す
                await UniTask.WaitUntil(() => RoomModel.GetInstance().IsEmpty, cancellationToken: token);
                ModelCache.Admin.OnReturnRoomTop();
            }
        }

        // 必要な情報をリセット
        MatchModel.GetInstance().Reset();
        PlayerRootObject.Instance.Reset();
        PlayerEquipmentModel.GetInstance().Reset();
        RoomModel.GetInstance().Reset();

        // RoomStateを戻す
        if(isAdmin)
        {
            if(hasConnecting)
            {
                RoomModel.GetInstance().OnRoomStateControllerSpawnOrReturn(RoomStateController.Instance);
            }
        }

        // Playerとして参加
        if(!isAdmin)
        {
            ModelCache.GetInstance().LoadAdminAs(false);
            roomName = await RoomListController.BeginSelectRoomAsync(token);

            if (roomName == GameConstant.EmptyRoomName)
            {
                var boolResult = false;
                CommonDialogController.Show($"select a null room?", onConfirm: () =>
                {
                    boolResult = true;
                });

                await UniTask.WaitUntil(() => boolResult, cancellationToken: token);
                return Unit.Default;
            }

            var runner = NetworkRunnerController.Runner;

            var startResult = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                PlayerCount = GameConstant.MaxPlayerPerRoom,
            });

            if (!startResult.Ok)
            {
                var boolResult = false;
                CommonDialogController.Show($"player joint srever error {startResult.ErrorMessage} - {startResult.ShutdownReason}", onConfirm: () =>
                {
                    boolResult = true;
                });

                await UniTask.WaitUntil(() => boolResult, cancellationToken: token);
                return Unit.Default;
            }
        }

        UniTask task1 = UniTask.WaitUntil(() => RoomStateController.Instance != null, cancellationToken: token)
        .ContinueWith(() =>
        {
            Debug.Log("@ player join room phase : RoomStateController");
            return UniTask.CompletedTask;
        });

        UniTask task2 = UniTask.WaitUntil(() => RpcConnector.Instance != null, cancellationToken: token)
                .ContinueWith(() =>
                {
                    Debug.Log("@ player join room phase : RpcConnector");
                    return UniTask.CompletedTask;
                });

        UniTask task3 = UniTask.WaitUntil(() => RoomModel.GetInstance().SelfPlayerRef != null, cancellationToken: token)
                .ContinueWith(() =>
                {
                    Debug.Log("@ player join room phase : SelfPlayerRef");
                    return UniTask.CompletedTask;
                });

        UniTask task4 = UniTask.WaitUntil(() => PlayerEquipmentModel.GetInstance().SelfEquipmentSetInfo != null, cancellationToken: token)
                .ContinueWith(() =>
                {
                    Debug.Log("@ player join room phase : SelfEquipmentSetInfo");
                    return UniTask.CompletedTask;
                });

        await UniTask.WhenAll(task1, task2, task3, task4);

        if(!isAdmin)
        {
            await PlayerRootObject.Instance.WaitAllObjectReadyAsync(token);

            RoomReadyController.gameObject.SetActive(true);
        }

        return Unit.Default;
    }
    public async UniTask<Unit> BeginSelect()
    {
        var token = this.destroyCancellationToken;
        var hasConnecting = RoomStateController.Instance;
        var playRole = PlayerRole.Player;
        var roomName = "";

        NetworkRunnerController.RefreshLocal();

        SoundManager.PlayBgm(bgmType: BgmType.Title);

        if (!hasConnecting)
        {
            (playRole, roomName) = await JoinSelectView.OpenViewAsync(token);
            var isAdmin = playRole == PlayerRole.Admin;
            GameCoreModel.Instance.IsAdminUser = isAdmin;
        }

        SoundManager.RequestLowSmoothPGM(token);

        if (GameCoreModel.Instance.IsAdminUser)
        {
            if (!hasConnecting)
            {
                ModelCache.GetInstance().LoadAdminAs(true);

                var runner = NetworkRunnerController.Runner;
                var startResult = await runner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                    SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                });

                if (!startResult.Ok)
                {
                    Debug.LogError($"admin start srever error {startResult.ErrorMessage} - {startResult.ShutdownReason}");
                }
            }
            else
            {
                await UniTask.WaitUntil(() => RoomModel.GetInstance().IsEmpty, cancellationToken: token);
                MatchModel.GetInstance().Reset();
                ModelCache.Admin.OnReturnRoomTop();
            }
        }
        else
        {
            RoomModel.GetInstance().Reset();
            PlayerEquipmentModel.GetInstance().Reset();
            PlayerRootObject.Instance.Reset();

            ModelCache.GetInstance().LoadAdminAs(false);
            roomName = await RoomListController.BeginSelectRoomAsync(token);

            if (roomName == GameConstant.EmptyRoomName)
            {
                return Unit.Default;
            }

            var runner = NetworkRunnerController.Runner;
            var startResult = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                PlayerCount = GameConstant.MaxPlayerPerRoom,
            });

            if (!startResult.Ok)
            {
                Debug.LogError($"player joint srever error {startResult.ErrorMessage} - {startResult.ShutdownReason}");
                return Unit.Default;
            }
        }


        await UniTask.WaitUntil(() =>
            RoomStateController.Instance != null &&
            RpcConnector.Instance != null &&
            RoomModel.GetInstance().SelfPlayerRef != null &&
            PlayerEquipmentModel.GetInstance().SelfEquipmentSetInfo != null,
            cancellationToken: token);

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            await PlayerRootObject.Instance.WaitAllObjectReadyAsync(token);

            RoomReadyController.gameObject.SetActive(true);
        }

        return Unit.Default;
    }
}

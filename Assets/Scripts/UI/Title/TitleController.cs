using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using System.Threading;
using StarMessage.Models;
using Fusion;
using UnityEngine.SceneManagement;
public class TitleController : MonoBehaviour
{
    [SerializeField] JoinPlayerTypeSelectView JoinSelectView;
    [SerializeField] RoomListController RoomListController;
    [SerializeField] RoomReadyController RoomReadyController;
    public void Start()
    {
        BeginSelect().Forget();
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
                GameCoreModel.Instance.AdminRoomId = roomName;

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
                ModelCache.Admin.OnReturnRoomTop();
            }
        }
        else
        {
            RoomModel.GetInstance().Reset();
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
            RoomModel.GetInstance().SelfPlayerRef != null,
            cancellationToken: token);

        if (!GameCoreModel.Instance.IsAdminUser)
        {
            await UniTask.WaitUntil(() => PlayerRootObject.Instance.SelfInfoObject != null, cancellationToken: token);
            RoomReadyController.gameObject.SetActive(true);
        }

        return Unit.Default;
    }
}

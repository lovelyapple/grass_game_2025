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
        var result = await JoinSelectView.OpenViewAsync(token);
        var isAdmin = result.Item1 == PlayerRole.Admin;
        GameCoreModel.Instance.IsAdminUser = isAdmin;

        if (isAdmin)
        {
            ModelCache.GetInstance().LoadAdminAs(true);
            GameCoreModel.Instance.AdminRoomId = result.Item2;

            var runner = NetworkRunnerController.Runner;
            var startResult = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = result.Item2,
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
            ModelCache.GetInstance().LoadAdminAs(false);
            var roomName = await RoomListController.BeginSelectRoomAsync(token);

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


        if (!isAdmin)
        {
            await UniTask.WaitUntil(() => PlayerRootObject.Instance.SelfInfoObject != null, cancellationToken: token);
            RoomReadyController.gameObject.SetActive(true);
        }

        return Unit.Default;
    }
}

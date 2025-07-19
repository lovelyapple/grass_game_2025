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
            GameCoreModel.Instance.AdminRoomId = result.Item2;

            var runner = NetworkRunnerController.Runner;
            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = result.Item2,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            });
        }
        else
        {
            var roomName = await RoomListController.BeginSelectRoomAsync(token);

            if(roomName == GameConstant.EmptyRoomName)
            {
                return Unit.Default;
            }

            var runner = NetworkRunnerController.Runner;
            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                PlayerCount = GameConstant.MaxPlayerPerRoom,
            });

            await UniTask.WaitUntil(() => 
            RoomStateController.Instance != null &&
            RoomModel.GetInstance().SelfPlayerRef != null,
            cancellationToken: token);

            RoomReadyController.gameObject.SetActive(true); 
        }

        return Unit.Default;
    }
}

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
    public void Start()
    {
        BeginSelect().Forget();
    }
    public async UniTask<Unit> BeginSelect()
    {
        var result = await JoinSelectView.OpenViewAsync(new CancellationToken());
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

        return Unit.Default;
    }
}

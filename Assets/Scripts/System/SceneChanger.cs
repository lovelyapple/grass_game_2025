using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;
public class SceneChanger : SingletonBase<SceneChanger>
{
    public enum SceneName
    {
        None,
        Initialize,
        Title,
        Game,
    }
    private readonly Dictionary<SceneName, int> _sceneIndex = new Dictionary<SceneName, int>
    {
        {SceneName.Initialize, 0},
        {SceneName.Title, 1},
        {SceneName.Game, 2},
    };
    
    private readonly Subject<SceneName> _onSceneChanged = new Subject<SceneName>();
    public Observable<SceneName> OnSceneChangedObservable() => _onSceneChanged;
    private SceneName _requestChangingScene = SceneName.None;
    public async UniTask<Unit> RequestChangeSceneAsyc(SceneName targetScene)
    {
        var requestTargetScene = targetScene;
        if (targetScene == SceneName.None || _requestChangingScene == targetScene)
        {
            _onSceneChanged.OnNext(requestTargetScene);
            return default;
        }

        _requestChangingScene = targetScene;
        await RequeChangeScene();

        _onSceneChanged.OnNext(requestTargetScene);
        return Unit.Default;
    }
    public async UniTask<Unit> RequeChangeScene()
    {
        await SceneManager.LoadSceneAsync(_sceneIndex[_requestChangingScene]);
        _requestChangingScene = SceneName.None;

        await UniTask.WaitForSeconds(1);
        return Unit.Default;
    }
}

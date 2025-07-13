using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;

namespace StarMessage.Models
{
    public class GameCoreModel : MonoBehaviour
    {
        private static GameCoreModel _instance;
        public static GameCoreModel Instance => _instance;
        //[SerializeField] private PlanetController PlanetController;
        public GamePhase CurrentGamePhase { get; private set; }
        public bool IsPlaying => CurrentGamePhase == GamePhase.Playing;
        private ModelCache _modelCache;
        public bool IsAdminUser = false;
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _instance = this;
            _modelCache = new ModelCache();
            _modelCache.SetInstance(_modelCache);
            _modelCache.InitializeModels(this.gameObject);

            SceneChanger.GetInstance().OnSceneChangedObservable()
                .Where(sceneName => sceneName == SceneChanger.SceneName.Title)
                .Subscribe(sceneName =>
                {
                    // LoadUIController.Close();
                })
                .AddTo(this);

            SceneChanger.GetInstance().OnSceneChangedObservable()
                .Where(sceneName => sceneName == SceneChanger.SceneName.Game)
                .SubscribeAwait(async (sceneName, token) =>
                {
                })
                .AddTo(this);
        }
        public void ResetGame()
        {
            CurrentGamePhase = GamePhase.Title;
        }
        public void RequestStartGame()
        {
            CurrentGamePhase = GamePhase.Ready;
        }
        public void OnGameResult(bool isWin)
        {
            CurrentGamePhase = GamePhase.Finished;
            PerformResult(isWin).Forget();
        }
        private async UniTask PerformResult(bool isWin)
        {
        }
        public void RestartGame()
        {
            ResetGame();
            RequestStartGame();
        }
        public void GotoTitle()
        {
            ResetGame();
            GotoTitleAsync(new CancellationToken());
        }
        private void GotoTitleAsync(CancellationToken cancellationToken)
        {
            SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Title).Forget();
        }
        private void GotoGameAsync(CancellationToken cancellationToken)
        {
            SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Game).Forget();
        }
    }
}

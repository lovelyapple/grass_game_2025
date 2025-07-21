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
        public string AdminRoomId;
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _instance = this;
            _modelCache = new ModelCache();
            _modelCache.SetInstance(_modelCache);
            _modelCache.InitializeModels(this.gameObject);
        }
        private void Start()
        {
            GotoTitle();
        }
        public void GotoTitle()
        {
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

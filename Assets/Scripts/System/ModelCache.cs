using R3;
using UnityEngine;
namespace StarMessage.Models
{
    public class ModelCache : SingletonBase<ModelCache>
    {
        private SceneChanger _sceneChanger;
        private RoomModel _roomModel;
        private RacingModel _racingModel;
        private PlayerEquipmentModel _equipMentModel;
        private GamePlayerInfoModel _gamePlayerModel;
        public void InitializeModels(GameObject coreModelObj)
        {
            _sceneChanger = new SceneChanger();
            _sceneChanger.SetInstance(_sceneChanger);

            _racingModel = new RacingModel();
            _racingModel.SetInstance(_racingModel);

            _roomModel = new RoomModel();
            _roomModel.SetInstance(_roomModel);

            _equipMentModel = new PlayerEquipmentModel();
            _equipMentModel.SetInstance(_equipMentModel);

            _roomModel.OnPlayerJoinObeservable()
            .Subscribe(x => _equipMentModel.OnPlayerJoined(x.Item1, x.Item2))
            .AddTo(coreModelObj);

            _roomModel.OnPlayerLeaveObservable()
            .Subscribe(x => _equipMentModel.OnPlayerLeaved(x))
            .AddTo(coreModelObj);

            _gamePlayerModel = new GamePlayerInfoModel();
            _gamePlayerModel.SetInstance(_gamePlayerModel);
        }
    }
}
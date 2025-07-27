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
        private MatchModel _matchModel;
        public static IGameAdminModel Admin { get; private set; }
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

            _roomModel.OnPlayerJoinObservable()
            .Subscribe(x => _equipMentModel.OnPlayerJoined(x.Item1, x.Item2))
            .AddTo(coreModelObj);

            _roomModel.OnPlayerLeaveObservable()
            .Subscribe(x => _equipMentModel.OnPlayerLeaved(x))
            .AddTo(coreModelObj);

            _gamePlayerModel = new GamePlayerInfoModel();
            _gamePlayerModel.SetInstance(_gamePlayerModel);

            _matchModel = new MatchModel();
            _matchModel.SetInstance(_matchModel);
        }
        public void LoadAdminAs(bool isAdmin)
        {
            if(isAdmin)
            {
                Admin = new GameAdminModel();
            }
            else
            {
                Admin = new NullGameAdminModel();
            }
        }
    }
}
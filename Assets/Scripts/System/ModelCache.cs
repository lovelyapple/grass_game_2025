using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StarMessage.Models
{
    public class ModelCache : SingletonBase<ModelCache>
    {
        private SceneChanger _sceneChanger;
        private RoomModel _roomModel;
        private RacingModel _racingModel; 
        public void InitializeModels(GameObject coreModelObj)
        {
            _sceneChanger = new SceneChanger();
            _sceneChanger.SetInstance(_sceneChanger);

            _racingModel = new RacingModel();
            _racingModel.SetInstance(_racingModel);

            _roomModel = new RoomModel();
            _roomModel.SetInstance(_roomModel);
        }
    }
}
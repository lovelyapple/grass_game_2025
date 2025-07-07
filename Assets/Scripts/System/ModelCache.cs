using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StarMessage.Models
{
    public class ModelCache : SingletonBase<ModelCache>
    {
        private SceneChanger _sceneChanger;
        public void InitializeModels(GameObject coreModelObj)
        {
            _sceneChanger = new SceneChanger();
            _sceneChanger.SetInsance(_sceneChanger);

        }
    }
}
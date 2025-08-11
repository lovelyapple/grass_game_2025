using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using System.Collections.Generic;

public class GameUISkillController : MonoBehaviour
{
    private List<GameObject> _spawnedOBjects = new List<GameObject>();
    private void Awake()
    {
        MatchModel.GetInstance().OnAnyOneUseSkillObservable()
        .Subscribe(type => SpawnSkillFX(type))
        .AddTo(this);
    }

    private void SpawnSkillFX(int skillType)
    {
        var prefab = ResourceContainer.Instance.GetStatusEffectFx((Characters)skillType);
        Instantiate(prefab, this.transform);
        _spawnedOBjects.Add(prefab);

        _spawnedOBjects.RemoveAll(x => x == null);
    }
    public void ClearAll()
    {
        foreach(var obj in _spawnedOBjects)
        {
            if(obj == null)
            {
                continue;
            }

            obj.SetActive(false);
            Destroy(obj);
        }

        _spawnedOBjects.Clear();
    }
}

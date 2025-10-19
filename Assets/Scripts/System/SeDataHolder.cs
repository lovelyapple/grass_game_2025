using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum SeType
{
    Cheers,
    MatchFinished,
    Button_Common_Se,
    Button_Confirm_Se,
    Se_Skill_Camera_Flash,
    Se_Skill_Car_Rush,
    Se_Skill_Drop_Down,
    Se_Count_Down_3,
    Se_Count_Down_2,
    Se_Count_Down_1,
    Se_Count_Down_0,
    Se_Barrer_broke,
    Max,
}
[CreateAssetMenu(fileName = "SeDataHolder", menuName = "Scriptable Objects/SeDataHolder")]
public class SeDataHolder : ScriptableObject
{
    [Serializable]
    public class SeData
    {
        public SeType Type;
        public AudioClip Clip; 
    }
    [SerializeField] List<SeData> SeDataList = new List<SeData>();
    public AudioClip GetSeData(SeType seType)
    {
        return SeDataList.FirstOrDefault(x => x.Type == seType).Clip;
    }
}

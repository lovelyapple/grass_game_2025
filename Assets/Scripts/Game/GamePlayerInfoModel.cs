using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GamePlayerInfoModel : SingletonBase<GamePlayerInfoModel>
{
    private const string Game_Player_Name_PrefsKey = "PalyerNameKey";
    private readonly string[] _randomNames = {
                            "JhonX92",
                            "Striker7",
                            "NeoWolf3",
                            "AceHunter",
                            "LunaByte",
                            "DarkZed9",
                            "SkyRex77",
                            "Valkyrie",
                            "NovaMax5",
                            "ZaneFire",
                            "EchoFox1",
                            "JetHawk2",
                            "RexBolt9",
                            "KiraBlade",
                            "FlintEX",
                            "Raynox3",
                            "ZeroJade",
                            "AshVolt1",
                            "HexLynx7",
                            "KaiStorm",};
    private string _selfName = "";
    public string SelfName
    {
        get { return GetDefaultRandomName(); }
        set
        {
            _selfName = value;
            if (!_randomNames.Contains(value))
            {
                PlayerPrefs.SetString(Game_Player_Name_PrefsKey, value);
            }
        }
    }
    public PlayerInfoObject SelfPlayerInfoObj { get; private set; }
    public void SetSelfObject(PlayerInfoObject playerInfoObject)
    {
        SelfPlayerInfoObj = playerInfoObject;
    }
    public void UpdateSelfEquipment(EquipmentSetInfo setInfo)
    {
        SelfPlayerInfoObj.PlayerEquipment = setInfo.ToStruct();
    }
    public string GetDefaultRandomName()
    {
        if (!string.IsNullOrEmpty(_selfName))
        {
            return _selfName;
        }

        _selfName = PlayerPrefs.GetString(Game_Player_Name_PrefsKey, "empty");

        if (_selfName != "empty" || string.IsNullOrEmpty(_selfName))
        {
            return _selfName;
        }

        int index = Random.Range(0, _randomNames.Length); // 注意: max は含まれない
        _selfName = _randomNames[index];
        return _selfName;
    }
}

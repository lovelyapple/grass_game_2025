using UnityEngine;
using UnityEngine.Rendering;

public class GamePlayerInfoModel : SingletonBase<GamePlayerInfoModel>
{
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
    public string SelfName = "Empty";
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
        int index = Random.Range(0, _randomNames.Length); // 注意: max は含まれない
        SelfName = _randomNames[index];
        return SelfName;
    }
}

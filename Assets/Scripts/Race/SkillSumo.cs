using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class SkillSumo : SkillBase
{
    public override void UseSkill()
    {
        PlayingSkill = true;
        base.UseSkill();
    }
    public override float SkillDuration() { return 1f; }
}

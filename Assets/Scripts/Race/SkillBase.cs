using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class SkillBase : MonoBehaviour
{
    public bool PlayingSkill;
    public FieldPlayerController PlayerController;
    public virtual void Init(FieldPlayerController fieldPlayerController)
    {
        PlayerController = fieldPlayerController;
    }
    public virtual float SkillDuration() { return 5f; }
    public virtual void UseSkill() { }
    public virtual void FinishPlaySkill() 
    {
        PlayingSkill = false;
    }
}

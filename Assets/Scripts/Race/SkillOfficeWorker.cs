using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class SkillOfficeWorker : SkillBase
{
    public override void UseSkill()
    {
        PlayingSkill = true;
        base.UseSkill();
        PerformSkill().Forget();
    }
    private async UniTask<Unit> PerformSkill()
    {
        var chara = PlayerController.GetCharaObj();
        chara.transform.eulerAngles = new Vector3(
            chara.transform.eulerAngles.x,
            chara.transform.eulerAngles.y,
            90f
        );

        try
        {
            await UniTask.WaitUntil(() => PlayingSkill == false, cancellationToken: this.gameObject.GetCancellationTokenOnDestroy());
        }
        finally
        {
            if (chara != null)
            {
                chara.transform.eulerAngles = new Vector3(
                    chara.transform.eulerAngles.x,
                    chara.transform.eulerAngles.y,
                    0
                );
            }
        }

        return Unit.Default;
    }
}

using UnityEngine;

public class FieldSkillAnimationWatcher : MonoBehaviour
{
    public void PlaySkillSe(SeType seType)
    {
        SoundManager.PlayOneShot(seType);
    }
    public void OnFinished()
    {
        Destroy(gameObject);
    }
}

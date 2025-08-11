using UnityEngine;

public class FieldSkillAnimationWatcher : MonoBehaviour
{
    public void OnFinished()
    {
        Destroy(gameObject);
    }
}

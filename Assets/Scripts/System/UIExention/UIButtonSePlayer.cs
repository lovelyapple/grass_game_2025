using R3;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSePlayer : MonoBehaviour
{
    [SerializeField] SeType Type = SeType.Button_Common_Se;
    private void Awake()
    {
        var button = GetComponent<Button>();

        if(button == null)
        {
            Debug.LogError($"can not find button go {gameObject.name} -- parent {transform.parent.name}");
            return;
        }

        button.OnClickAsObservable()
        .Subscribe(x => SoundManager.PlayOneShot(Type))
        .AddTo(this);
    }
}

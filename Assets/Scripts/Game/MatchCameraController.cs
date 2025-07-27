using UnityEngine;

public class MatchCameraController : MonoBehaviour
{
    public static MatchCameraController Instance { get; private set; }
    [SerializeField] private Transform target;  // プレイヤーのTransform
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float fixedY = 5f; // 固定するY座標
    [SerializeField] private float fixedX = -10f; // 固定するZ座標
    private void Awake()
    {
        Instance = this;
    }
    void LateUpdate()
    {
        if(target == null)
        {
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(
            fixedX,  // Xだけ追従
            fixedY,
            target.transform.position.z
        );

        // 線形補間でスムーズに追従
        transform.position = Vector3.Lerp(currentPos, targetPos, followSpeed * Time.deltaTime);
    }
    public void SetupTarget(Transform trn)
    {
        target = trn;
    }
}

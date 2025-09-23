using UnityEngine;

public class MatchObjectFollower : MonoBehaviour
{
    [SerializeField] private Transform fieldObject;   // 追従対象
    [SerializeField] private RectTransform uiMarker;  // UIマーカー
    [SerializeField] private Canvas canvas;           // UI用Canvas
    [SerializeField] private float upOffset = 50f;
    [SerializeField] private Camera UICamera;
    [SerializeField] int FixUpdate = 0;
    public void Setup(Transform playerTransform)
    {
        fieldObject = playerTransform;
    }
    void LateUpdate()
    {
        if (FixUpdate == 0) return;
        if (fieldObject == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(fieldObject.position);
        Vector3 screenPos = UICamera.ViewportToScreenPoint(viewportPos);

        if (screenPos.z > 0) // カメラの前にある場合のみ
        {
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                UICamera,
                out uiPos
            );

            uiMarker.localPosition = uiPos + Vector2.up * upOffset;
            uiMarker.gameObject.SetActive(true);
        }
        else
        {
            uiMarker.gameObject.SetActive(false); // 背後にある場合は非表示
        }
    }
    void FixedUpdate()
    {
        if (FixUpdate == 1) return;
        if (fieldObject == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(fieldObject.position);
        Vector3 screenPos = UICamera.ViewportToScreenPoint(viewportPos);

        if (screenPos.z > 0) // カメラの前にある場合のみ
        {
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                UICamera,
                // canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out uiPos
            );

            uiMarker.localPosition = uiPos + Vector2.up * upOffset;
            // uiMarker.gameObject.SetActive(true);
        }
        else
        {
            // uiMarker.gameObject.SetActive(false); // 背後にある場合は非表示
        }
    }
}

using UnityEngine;

public class VerticalCameraShake : MonoBehaviour
{
    [Header("Default Params")]
    public float defaultDuration = 0.2f;
    public float defaultAmplitude = 0.2f; // 揺れ幅（Y）
    public float defaultFrequency = 18f;  // 揺れ速さ(Hz)
    public bool useUnscaledTime = true;

    Vector3 _baseLocalPos;
    float _timeLeft;
    float _amp;
    float _freq;

    void Awake() => _baseLocalPos = transform.localPosition;

    void OnEnable()
    {
        _baseLocalPos = transform.localPosition; // 有効化時に基準更新
        StopShake(); // 念のためリセット
    }

    void LateUpdate()
    {
        if (_timeLeft <= 0f) return;

        float t = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _timeLeft -= t;

        // 0→1で減衰（イージング：スムーズに終わる）
        float progress = Mathf.Clamp01(1f - (_timeLeft / Mathf.Max(0.0001f, _currentDuration)));
        float damping = 1f - progress * progress; // (1 - p^2)

        float phase = (_useTime + t) * _freq * Mathf.PI * 2f;
        _useTime += t;

        float yOffset = Mathf.Sin(phase) * _amp * damping;
        transform.localPosition = _baseLocalPos + new Vector3(0f, yOffset, 0f);

        if (_timeLeft <= 0f) // 終了時に原点へ
            transform.localPosition = _baseLocalPos;
    }

    // --- API ---
    float _currentDuration, _useTime;

    public void Shake(float duration, float amplitude, float frequency)
    {
        _currentDuration = _timeLeft = duration;
        _amp = amplitude;
        _freq = frequency;
        _useTime = 0f;
    }

    public void Shake() => Shake(defaultDuration, defaultAmplitude, defaultFrequency);

    public void StopShake()
    {
        _timeLeft = 0f;
        transform.localPosition = _baseLocalPos;
    }
}
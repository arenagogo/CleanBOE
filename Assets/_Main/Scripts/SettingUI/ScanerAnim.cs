using UnityEngine;
using DG.Tweening;

public class ScanerAnim : MonoBehaviour
{
    [Header("Scanner Settings")]
    [Tooltip("RectTransform of the scanning line. If null, the component's RectTransform will be used.")]
    public RectTransform lineScaner;

    [Tooltip("Total vertical travel distance in local Y units (distance from bottom to top).")]
    public float travelDistance = 52f;

    [Tooltip("Time in seconds to move from bottom to top.")]
    public float duration = 1f;

    [Tooltip("Delay before the animation starts (seconds).")]
    public float startDelay = 0f;

    [Tooltip("Whether the animation should start automatically when the object is enabled.")]
    public bool startOnEnable = true;

    [Tooltip("Easing used for the tween.")]
    public Ease ease = Ease.InOutSine;

    private Tween _tween;
    private float _startY;

    private void Awake()
    {
        if (lineScaner == null)
            lineScaner = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (startOnEnable)
            StartLoop();
    }

    private void OnDisable()
    {
        StopLoop();
    }

    public void StartLoop()
    {
        if (lineScaner == null)
            return;

        _startY = lineScaner.localPosition.y;
        float targetY = _startY + travelDistance;

        _tween?.Kill();
        _tween = lineScaner.DOLocalMoveY(targetY, duration)
            .SetEase(ease)
            .SetDelay(startDelay)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);
    }

    public void StopLoop()
    {
        if (_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }
    }

    public void Play() => StartLoop();
    public void Pause() => _tween?.Pause();
    public void Resume() => _tween?.Play();
    public void Stop() => StopLoop();
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnimVS : MonoBehaviour
{
    public RectTransform avatarLocal;
    public RectTransform avatarRemote;
    public RectTransform versusImg;
    public RectTransform textGameMode;

    [Header("Animation Settings")]
    public float moveDuration = 0.6f;
    public float scaleDuration = 0.35f;
    public float avatarOffset = 800f; // how far offscreen to start
    public Ease moveEase = Ease.OutBack;
    public Ease scaleEase = Ease.OutBack;

    private Vector2 _localOrigPos;
    private Vector2 _remoteOrigPos;
    private Vector3 _versusOrigScale;
    private Vector3 _textOrigScale;
    private Sequence _sequence;

    public CanvasGroup cvs;

    public SimpleAgoraController_Unified controller;

    // Completion now logs debug message by default

    void Start()
    {
        // StarAnim();
    }

    public void StarAnim()
    {
        cvs.alpha = 1;
        // Stop previous animation if any
        _sequence?.Kill();

        // Ensure we have originals
        if (avatarLocal != null) avatarLocal.anchoredPosition = _localOrigPos + new Vector2(avatarOffset, 0);
        if (avatarRemote != null) avatarRemote.anchoredPosition = _remoteOrigPos - new Vector2(avatarOffset, 0);

        if (versusImg != null) versusImg.localScale = Vector3.zero;
        if (textGameMode != null) textGameMode.localScale = Vector3.zero;

        _sequence = DOTween.Sequence();

        // Move avatars simultaneously
        if (avatarLocal != null && avatarRemote != null)
        {
            _sequence.Join(avatarLocal.DOAnchorPos(_localOrigPos, moveDuration).SetEase(moveEase));
            _sequence.Join(avatarRemote.DOAnchorPos(_remoteOrigPos, moveDuration).SetEase(moveEase));
        }
        else
        {
            if (avatarLocal != null)
                _sequence.Append(avatarLocal.DOAnchorPos(_localOrigPos, moveDuration).SetEase(moveEase));
            if (avatarRemote != null)
                _sequence.Append(avatarRemote.DOAnchorPos(_remoteOrigPos, moveDuration).SetEase(moveEase));
        }

        // Scale versus image then game mode text
        if (versusImg != null)
            _sequence.Append(versusImg.DOScale(_versusOrigScale, scaleDuration).SetEase(scaleEase));

        if (textGameMode != null)
            _sequence.Append(textGameMode.DOScale(_textOrigScale, scaleDuration).SetEase(scaleEase));

        // Log debug message when sequence completes
        _sequence.OnComplete(() => Oncomplete());

        _sequence.Play();
    }

    void Oncomplete()
    {
        Invoke(nameof(OncompleteX), 2f);
    }

    void OncompleteX()
    {
        controller.DelayShowCategory();
    }

    private void Awake()
    {
        if (avatarLocal != null) _localOrigPos = avatarLocal.anchoredPosition;
        if (avatarRemote != null) _remoteOrigPos = avatarRemote.anchoredPosition;
        if (versusImg != null) _versusOrigScale = versusImg.localScale;
        else _versusOrigScale = Vector3.one;
        if (textGameMode != null) _textOrigScale = textGameMode.localScale;
        else _textOrigScale = Vector3.one;
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }

    // Optional control methods
    public void StopAnim()
    {
        _sequence?.Kill();
    }
}

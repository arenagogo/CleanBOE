using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimResult : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform iconTitle;
    public RectTransform tiitle;
    public TextMeshProUGUI tiitleText;
    public RectTransform statusGame; // "YOU WIN" text
    public TextMeshProUGUI statusText;
    public RectTransform score;
    public TextMeshProUGUI scoreText;
    public RectTransform imgCoin;
    public RectTransform buttonOk;

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f;

    [Header("Animation Settings")]
    public float sideOffset = 500f; // Jarak elemen bergeser ke samping
    public float animationDuration = 0.6f; // Durasi animasi geser

    Vector2 iconPos;
    Vector2 titlePos;
    Vector2 statusPos;
    Vector2 scorePos;
    Vector2 coinPos;
    Vector2 okPos;

    [SerializeField] private Ease ease;

    void Start()
    {
        iconPos = iconTitle.anchoredPosition;
        titlePos = tiitle.anchoredPosition;
        statusPos = statusGame.anchoredPosition;
        scorePos = score.anchoredPosition;
        coinPos = imgCoin.anchoredPosition;
        okPos = buttonOk.anchoredPosition;
        iconTitle.anchoredPosition = new Vector2(iconPos.x - sideOffset, iconPos.y);
        statusGame.anchoredPosition = new Vector2(statusPos.x - sideOffset, statusPos.y);
        imgCoin.anchoredPosition = new Vector2(coinPos.x - sideOffset, coinPos.y);
        tiitle.anchoredPosition = new Vector2(titlePos.x + sideOffset, titlePos.y);
        score.anchoredPosition = new Vector2(scorePos.x + sideOffset, scorePos.y);
        buttonOk.anchoredPosition = new Vector2(okPos.x + sideOffset, okPos.y);
    }

    public void PlayAnimation()
    {
        iconTitle.DOAnchorPos(iconPos, 0.5f).SetEase(ease);
        statusGame.DOAnchorPos(statusPos, 0.5f).SetEase(ease);
        imgCoin.DOAnchorPos(coinPos, 0.5f).SetEase(ease);

        tiitle.DOAnchorPos(titlePos, 0.5f).SetEase(ease);
        score.DOAnchorPos(scorePos, 0.5f).SetEase(ease);
        buttonOk.DOAnchorPos(okPos, 0.5f).SetEase(ease);
    }
}

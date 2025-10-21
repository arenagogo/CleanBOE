using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Sprite[] tutorial;
    public Image imgTutor;
    public float fadeDuration = 0.8f; // durasi fade in/out
    public float displayDuration = 1.5f; // waktu tiap gambar tampil penuh

    private int currentIndex = 0;

    void Start()
    {
        if (tutorial.Length > 0)
        {
            imgTutor.sprite = tutorial[0];
            imgTutor.color = new Color(1, 1, 1, 0); // mulai transparan
            SetAnimation();
        }
    }

    void SetAnimation()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(imgTutor.DOFade(1f, fadeDuration)) // fade in
           .AppendInterval(displayDuration)           // tunggu sebentar
           .Append(imgTutor.DOFade(0f, fadeDuration)) // fade out
           .OnComplete(() =>
           {
               // ganti sprite
               currentIndex = (currentIndex + 1) % tutorial.Length;
               imgTutor.sprite = tutorial[currentIndex];
               // ulang animasi
               SetAnimation();
           });
    }
}

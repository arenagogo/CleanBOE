using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }
    public Effect[] effects;

    public int currentValue = 100;
    private int lastHighValue = 100;
    private float lastHighTime = 0f;
    public int dropThreshold = 30;
    public float timeWindow = 5f;

    public void SetValue(float vl)
    {
        //  float v = vl * 100;
        currentValue = (int)vl;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (Effect effect in effects)
        {
            effect.particleSystem = effect.effect.GetComponent<ParticleSystem>();
        }

        // SpawnEffect("tengkorak");
    }

    public void SpawnEffect(string effectName)
    {
        foreach (Effect item in effects)
        {
            if (item.name == effectName)
            {
                item.effect.SetActive(true);
                item.particleSystem.Play();
                StartCoroutine(StopEffect(item.particleSystem, item.effect));
                break;
            }
        }
    }


    IEnumerator StopEffect(ParticleSystem particle, GameObject particleObject)
    {
        yield return new WaitForSeconds(5);
        if (particle != null && particle.isPlaying)
        {
            particle.Stop();
        }
        particleObject.SetActive(false);
    }

    void Update()
    {
        DetectDrasticDrop();
    }

    void DetectDrasticDrop()
    {
        // Jika nilai saat ini lebih tinggi dari nilai sebelumnya, update nilai tinggi terakhir
        if (currentValue > lastHighValue)
        {
            lastHighValue = currentValue;
            lastHighTime = Time.time;
        }

        // Jika nilai drop lebih dari threshold
        if (lastHighValue - currentValue >= dropThreshold)
        {
            float elapsed = Time.time - lastHighTime;

            if (elapsed <= timeWindow)
            {
                Debug.Log($"⚠️ Drop drastis terdeteksi! {lastHighValue} → {currentValue} dalam {elapsed:F2} detik");
                SpawnEffect("listrik");
                // Reset agar tidak spam log
                lastHighValue = currentValue;
                lastHighTime = Time.time;
            }
        }
    }

    [Serializable]
    public class Effect
    {
        public string name;
        public GameObject effect;
        public ParticleSystem particleSystem;
    }
}

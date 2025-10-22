using UnityEngine;

public class PlayerEmotionAnalyzer : MonoBehaviour
{
    public static PlayerEmotionAnalyzer instance;

    [System.Serializable]
    public class EmotionResult
    {
        public string title;
        public Color color;

        public EmotionResult(string title, Color color)
        {
            this.title = title;
            this.color = color;
        }
    }

    [Header("Particle Aura")]
    public ParticleSystem[] auraParticle; // array efek aura



    void Awake()
    {
        instance = this;
    }
    public EmotionResult AnalyzePlayerEmotion(int totalGames, int totalWins)
    {
        if (totalGames <= 0)
            return new EmotionResult("None (New Player)", Color.gray);

        float winRate = (float)totalWins / totalGames * 100f;

        // 🔹 Title rules
        if (totalGames < 5)
        {
            return new EmotionResult("None (New Player)", Color.gray);
        }
        else if (totalGames >= 5 && winRate < 40f)
        {
            return new EmotionResult("Calm", new Color(0.2f, 0.4f, 1f)); // Biru
        }
        else if (totalGames >= 10 && winRate < 60f)
        {
            return new EmotionResult("Passionate", Color.red);
        }
        else if (totalGames >= 15 && winRate < 80f)
        {
            return new EmotionResult("Confident", new Color(1f, 0.84f, 0f)); // Emas
        }
        else if (totalGames >= 20 && winRate >= 80f)
        {
            return new EmotionResult("Chaotic", new Color(0.6f, 0f, 0.9f)); // Ungu
        }

        // Default fallback
        return new EmotionResult("None (New Player)", Color.gray);
    }

    // 🔹 Contoh penggunaan
    // void Start()
    // {
    //     int totalGames = 22;
    //     int totalWins = 10;

    //     EmotionResult result = AnalyzePlayerEmotion(totalGames, totalWins);
    //     Debug.Log($"🎯 Title: {result.title} | Color: {result.color}");

    //     ApplyAuraColor(result.color);
    // }

    public void SetAura(int totalGames, int totalWins)
    {
        EmotionResult result = AnalyzePlayerEmotion(totalGames, totalWins);
        Debug.Log($"🎯 Title: {result.title} | Color: {result.color} win {totalWins} total game {totalGames}");

        ApplyAuraColor(result.color);
    }

    /// <summary>
    /// Terapkan warna hasil analisis ke semua particle aura
    /// </summary>
    void ApplyAuraColor(Color color)
    {
        if (auraParticle == null || auraParticle.Length == 0)
            return;

        foreach (var ps in auraParticle)
        {
            if (ps == null) continue;

            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);

            // Jika partikel sedang aktif, restart biar efek langsung berubah
            ps.Clear();
            ps.Play();
        }
    }
}

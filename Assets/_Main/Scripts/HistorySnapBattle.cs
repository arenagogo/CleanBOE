using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HistorySnapBattle : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI status;
    public TextMeshProUGUI scoreOnGame;
    public TextMeshProUGUI number;
    public Button buttonDetail;

    public void SetDataHistory(string date, string statusText, string score, string numberText)
    {
        if (dateText != null)
            dateText.text = date;
        if (status != null)
            status.text = statusText;
        if (scoreOnGame != null)
            scoreOnGame.text = score;
        if (number != null)
            number.text = numberText;
    }
}

using TMPro;
using UnityEngine;

public class ShowHidePassword : MonoBehaviour
{
    [Header("🎯 Target Field")]
    public TMP_InputField tMP_InputField;

    [Header("👁️ Toggle Label (SHOW / HIDE)")]
    public TextMeshProUGUI info;

    public void _ShowHidePassword()
    {
        if (tMP_InputField.contentType == TMP_InputField.ContentType.Password)
        {
            // Ubah ke mode teks biasa
            tMP_InputField.contentType = TMP_InputField.ContentType.Standard;
            string currentText = tMP_InputField.text;
            tMP_InputField.text = currentText;
            tMP_InputField.ForceLabelUpdate();

            if (info != null) info.text = "HIDE";
        }
        else
        {
            // Ubah ke mode password
            tMP_InputField.contentType = TMP_InputField.ContentType.Password;
            string currentText = tMP_InputField.text;
            tMP_InputField.text = currentText;
            tMP_InputField.ForceLabelUpdate();

            if (info != null) info.text = "SHOW";
        }
    }
}

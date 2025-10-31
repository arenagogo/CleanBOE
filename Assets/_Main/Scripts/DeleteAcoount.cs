using UnityEngine;
using TMPro;

public class DeleteAcoount : MonoBehaviour
{
    public SimpleAgoraController_Unified controller;

    public TextMeshProUGUI textStatus;
    public CanvasGroup notifSuccessDelete;
    public void DeleteAccount()
    {
        string apiDelete = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/users/account";
        controller.DeleteData(apiDelete, controller.data.data.accessToken,
                    (json) =>
                    {
                        Debug.Log("Data agora available loaded: " + json);
                        try
                        {
                            Debug.Log("Account deleted successfully.");
                            notifSuccessDelete.alpha = 1;
                            notifSuccessDelete.interactable = true;
                            notifSuccessDelete.blocksRaycasts = true;
                            textStatus.text = "Your account has been successfully deleted.";

                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Failed to DeleteAccount: " + e.Message);
                            notifSuccessDelete.alpha = 1;
                            notifSuccessDelete.interactable = true;
                            notifSuccessDelete.blocksRaycasts = true;
                            textStatus.text = "An error occurred while deleting your account. Please try again later.";
                        }
                    },
                    (error) =>
                    {
                        Debug.LogError("Failed to DeleteAccount: " + error);
                        notifSuccessDelete.alpha = 1;
                        notifSuccessDelete.interactable = true;
                        notifSuccessDelete.blocksRaycasts = true;
                        textStatus.text = "An error occurred while deleting your account. Please try again later.";
                    }
                );
    }
}

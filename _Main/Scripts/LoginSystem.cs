using System.Collections;
using System.Collections.Generic;
using System.Text;
using Agora.Rtc.LitJson;
using ArenaGo.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    public MainMenuSnapBattle mainMenuSnap;
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private SimpleAgoraController_Unified controler;
    private string keyDataJwt = "dataJwt";
    public Button btnLogin;





    private bool isOkRegister = false;
    private bool ValidateRegisterInput()
    {
        // Reset flag
        isOkRegister = false;

        // Get input values
        string email = inputEmailRegister.text.Trim();
        string password = inputPasswordRegister.text;
        string repeatPassword = inputRepeatPasswordRegister.text.Trim();
        string phone = inputPhoneNumberRegister.text.Trim();
        string username = inputUsername.text.Trim();

        // Clear all validation messages
        notifValEmail.text = "";
        notifValPassword.text = "";
        notifValReapeatPassword.text = "";
        notifValPhoneNumber.text = "";
        notifValUsername.text = "";

        // === Email Validation ===
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email cannot be empty");
            notifValEmail.text = "Email cannot be empty";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            Debug.LogError("Invalid email format");
            notifValEmail.text = "Invalid email format";
            return false;
        }

        // === Password Validation ===
        if (string.IsNullOrEmpty(password))
        {
            Debug.LogError("Password cannot be empty");
            notifValPassword.text = "Password cannot be empty";
            return false;
        }

        if (password.Length < 6)
        {
            Debug.LogError("Password must be at least 6 characters long");
            notifValPassword.text = "Password must be at least 6 characters long";
            return false;
        }

        // === Repeat Password Validation ===
        if (string.IsNullOrEmpty(repeatPassword))
        {
            Debug.LogError("Repeat Password cannot be empty");
            notifValReapeatPassword.text = "Repeat Password cannot be empty";
            return false;
        }

        if (password != repeatPassword)
        {
            Debug.LogError("Passwords do not match");
            notifValReapeatPassword.text = "Passwords do not match";
            return false;
        }

        // === Phone Number Validation ===
        if (string.IsNullOrEmpty(phone))
        {
            Debug.LogError("Phone number cannot be empty");
            notifValPhoneNumber.text = "Phone number cannot be empty";
            return false;
        }

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Username cannot be empty");
            notifValUsername.text = "Username cannot be empty";
            return false;
        }

        if (username.Contains(" "))
        {
            Debug.LogError("Username can only contain letters, numbers, and underscores");
            notifValUsername.text = "Username can only contain letters, numbers, and underscores";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
        {
            Debug.LogError("Username can only contain letters, numbers, and underscores");
            notifValUsername.text = "Username can only contain letters, numbers, and underscores";
            return false;
        }

        if (username.Length < 6)
        {
            Debug.LogError("Username must be at least 6 characters long");
            notifValUsername.text = "Username must be at least 6 characters long";
            return false;
        }



        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^08[1-9][0-9]{7,10}$"))
        {
            Debug.LogError("Invalid phone number format");
            notifValPhoneNumber.text = "Phone number must start with 08 and contain 10–13 digits (e.g., 08xxxxxxxxx)";
            return false;
        }

        if (phone.Length < 9 || phone.Length > 15)
        {
            Debug.LogError("Invalid phone number length");
            notifValPhoneNumber.text = "Phone number must be between 9–15 digits";
            return false;
        }

        // === Username Validation ===




        // ✅ All validation passed
        isOkRegister = true;
        return true;
    }


    public TextMeshProUGUI notifValEmailLogin;
    public TextMeshProUGUI notifValPasswordLogin;

    private bool isOkLogin = false;

    private bool ValidateLoginInput()
    {
        // Reset flag
        isOkLogin = false;

        // Get input values
        string email = inputEmail.text.Trim();
        string password = inputPassword.text;


        // Clear all validation messages
        notifValEmailLogin.text = "";
        notifValPasswordLogin.text = "";

        // === Email Validation ===
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email cannot be empty");
            notifValEmailLogin.text = "Email cannot be empty";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            Debug.LogError("Invalid email format");
            notifValEmailLogin.text = "Invalid email format";
            return false;
        }

        // === Password Validation ===
        if (string.IsNullOrEmpty(password))
        {
            Debug.LogError("Password cannot be empty");
            notifValPasswordLogin.text = "Password cannot be empty";
            return false;
        }
        isOkLogin = true;
        return true;
    }




    private void Awake()
    {
        if (PlayerPrefs.HasKey(keyDataJwt))
        {
            string json = PlayerPrefs.GetString(keyDataJwt);
            GetdataJWT(json, false);
            mainMenuSnap.statusLogin = true;
        }
        else
        {
            mainMenuSnap.statusLogin = false;
        }
    }

    public void Login()
    {
        if (ValidateLoginInput() == false)
            return;
        btnLogin.interactable = false;
        string apiLoginUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/auth/login-custom-token";
        LoginStructur loginStructur = new LoginStructur(inputEmail.text, inputPassword.text);
        string rawBody = JsonMapper.ToJson(loginStructur);

        controler.PostJson(apiLoginUrl, string.Empty, rawBody,
            (json) =>
            {
                PlayerPrefs.SetString(keyDataJwt, json);
                GetdataJWT(json, true);

            },
            (err) => ErrorLogin(err)
        );

    }

    void ErrorLogin(string error)
    {
        notifValEmailLogin.text = "check your email and password";
        notifValPasswordLogin.text = "check your email and password";
        Debug.LogError("Failed Login " + error);
        btnLogin.interactable = true;
        // inputEmail.text = string.Empty;
        // inputPassword.text = string.Empty;
        Loading.instance.HideLoading();
        PlayerPrefs.DeleteKey("dataJwt");
    }

    void GetdataJWT(string jsonJwt, bool xx)
    {
        controler.data = JsonUtility.FromJson<LoginResponse>(jsonJwt);

        StartCoroutine(controler.GetdataAccount(xx));
    }

    public class LoginStructur
    {
        public string email; public string password;
        public LoginStructur(string _email, string _password)
        {

            email = _email;
            password = _password;
        }
    }

    //=========REGISTER=========

    public TMP_InputField inputEmailRegister;
    public TMP_InputField inputPasswordRegister;
    public TMP_InputField inputRepeatPasswordRegister;
    public TMP_InputField inputPhoneNumberRegister;
    public TMP_InputField inputUsername;

    public TextMeshProUGUI notifValEmail;
    public TextMeshProUGUI notifValPassword;
    public TextMeshProUGUI notifValReapeatPassword;
    public TextMeshProUGUI notifValUsername;
    public TextMeshProUGUI notifValPhoneNumber;


    public Button btnRegister;
    public Button btnBackToLogin;

    public CanvasGroup loginCvs;
    public CanvasGroup registerCvs;

    public void OpenRegister()
    {
        loginCvs.alpha = 0;
        loginCvs.blocksRaycasts = false;
        loginCvs.interactable = false;

        registerCvs.alpha = 1;
        registerCvs.blocksRaycasts = true;
        registerCvs.interactable = true;
    }

    public void OpenLogin()
    {
        loginCvs.alpha = 1;
        loginCvs.blocksRaycasts = true;
        loginCvs.interactable = true;

        registerCvs.alpha = 0;
        registerCvs.blocksRaycasts = false;
        registerCvs.interactable = false;
    }

    public void Register()
    {
        if (ValidateRegisterInput() == false)
            return;

        Loading.instance.ShowLoading();
        if (inputPasswordRegister.text != inputRepeatPasswordRegister.text)
        {
            Debug.LogError("Password not match");
            return;
        }

        StartCoroutine(CallApiWithBasicAuth());
    }

    void ErrorRegis(string msg)
    {
        notifFailRegister.SetActive(true);
        textNotifFailRegister.text = msg;
    }

    void ErrorRegisSystem(string error)
    {
        Debug.LogError("Failed Register " + error);
    }

    public void AutoLogin(string email, string password)
    {
        btnLogin.interactable = false;
        string apiLoginUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/auth/login-custom-token";
        LoginStructur loginStructur = new LoginStructur(email, password);
        string rawBody = JsonMapper.ToJson(loginStructur);

        controler.PostJson(apiLoginUrl, string.Empty, rawBody,
            (json) =>
            {
                PlayerPrefs.SetString(keyDataJwt, json);
                SceneManager.LoadSceneAsync("MainSceneAgora");
            },
            (err) => ErrorLogin(err)
        );

        MainMenuSnapBattle.Instance.OpenAccounMenu(true);
    }

    public GameObject notifFailRegister;
    public TextMeshProUGUI textNotifFailRegister;
    public class RegisterStructur
    {
        public string email;
        public string password;
        public string phoneNumber;
        // public string displayName;
        public string username;
        public RegisterStructur(string _email, string _password, string _phoneNumber, string _name)
        {
            email = _email;
            password = _password;
            phoneNumber = _phoneNumber;
            username = _name;
        }
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
        public string error;
        public int statusCode;
    }

    IEnumerator CallApiWithBasicAuth()
    {
        registerCvs.DOFade(0, 0.1f);
        string apiRegisterUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/auth/register";

        // Payload body JSON
        RegisterStructur payload = new RegisterStructur(inputEmailRegister.text, inputPasswordRegister.text, inputPhoneNumberRegister.text, inputUsername.text);

        string jsonBody = JsonUtility.ToJson(payload);

        Debug.Log("Register Payload: " + jsonBody);

        // Basic Auth
        string username = "arenagoapi";
        string password = "arenago707";
        string auth = username + ":" + password;
        string authBase64 = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

        // Request POST
        UnityWebRequest req = new UnityWebRequest(apiRegisterUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Basic " + authBase64);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error: " + req.error + "  __  " + req.downloadHandler.text);
            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);
            notifFailRegister.SetActive(true);
            textNotifFailRegister.text = errorResponse.message;
            registerCvs.DOFade(1, 0.5f);
        }
        else
        {
            Debug.Log("✅ Success: " + req.downloadHandler.text);
            // AutoLogin(inputEmailRegister.text, inputPasswordRegister.text);
            registerCvs.alpha = 0;
            registerCvs.interactable = false;
            registerCvs.blocksRaycasts = false;
            loginCvs.DOFade(1, 0.5f);
            loginCvs.interactable = true;
            loginCvs.blocksRaycasts = true;

            inputEmail.text = string.Empty;
            inputPassword.text = string.Empty;
            notifValEmailLogin.text = string.Empty;
            notifValPasswordLogin.text = string.Empty;


            inputEmailRegister.text = string.Empty;
            inputPasswordRegister.text = string.Empty;
            inputRepeatPasswordRegister.text = string.Empty;
            inputPhoneNumberRegister.text = string.Empty;
            inputUsername.text = string.Empty;

            notifValEmail.text = string.Empty;
            notifValPassword.text = string.Empty;
            notifValReapeatPassword.text = string.Empty;
            notifValPhoneNumber.text = string.Empty;
            notifValUsername.text = string.Empty;

            suksesLoginPopup.SetActive(true);

        }


        Loading.instance.HideLoading();
    }

    public GameObject suksesLoginPopup;


}

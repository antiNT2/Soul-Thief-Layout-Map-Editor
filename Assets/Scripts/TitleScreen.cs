using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TitleScreen : MonoBehaviour
{
    CustomNetworkManager CustomNetworkManager;
    [SerializeField]
    Text versionDisplay;
    [SerializeField]
    GameObject onlineButtons;
    [SerializeField]
    GameObject normalButtons;
    [SerializeField]
    InputField ipAdress;

    [Header("Loading Panel")]
    [SerializeField]
    GameObject loadingPanel;
    [SerializeField]
    GameObject loadingIcon;
    [SerializeField]
    Text loadingStatuts;
    [SerializeField]
    Button okButton;

    public static TitleScreen instance;

    private void Start()
    {
        instance = this;

        CustomNetworkManager = GameObject.FindObjectOfType<CustomNetworkManager>();
        Cursor.visible = true;
        versionDisplay.text = "v " + Application.version;
        CustomNetworkManager.serverBindToIP = false;
        ipAdress.text = PlayerPrefs.GetString("IP");

        if (System.Environment.GetCommandLineArgs().Length == 2)
        {
            if (PlayerPrefs.GetString("OpenFile") != "done")
            {
                PlayerPrefs.SetString("OpenFile", System.Environment.GetCommandLineArgs()[1]);
                Local();
            }
        }
        else
        {
            PlayerPrefs.SetString("OpenFile", "");
        }

    }

    public void Host()
    {
        if (ipAdress.text.Length < 8)
        {
            ShowError("Incorrect IP Adress");
        }
        else
        {
            CustomNetworkManager.serverBindToIP = true;
            CustomNetworkManager.serverBindAddress = ipAdress.text;
            CustomNetworkManager.networkPort = 7676;
            CustomNetworkManager.StartHost();
            PlayerPrefs.SetString("IP", ipAdress.text);
            loadingStatuts.text = "Starting Server...";
            StartCoroutine(LoadThenError("Couldn't start server on adress " + ipAdress.text, 3f));
        }
    }
    public void Client()
    {
        if (ipAdress.text.Length < 8)
        {
            ShowError("Incorrect IP Adress");
        }
        else
        {
            CustomNetworkManager.networkAddress = ipAdress.text;
            CustomNetworkManager.networkPort = 7676;
            CustomNetworkManager.StartClient();
            PlayerPrefs.SetString("IP", ipAdress.text);
            loadingStatuts.text = "Connecting to " + ipAdress.text + "...";
            StartCoroutine(LoadThenError("", 4f));
        }
    }

    public void ShowError(string _error)
    {
        loadingPanel.SetActive(true);
        loadingIcon.SetActive(false);
        loadingStatuts.text = _error;
        okButton.gameObject.SetActive(true);
    }

    IEnumerator LoadThenError(string _error, float waitTime)
    {
        loadingPanel.SetActive(true);
        loadingIcon.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        if (_error != "")
            ShowError(_error);
    }

    public void OkButton()
    {
        loadingPanel.SetActive(false);
        okButton.gameObject.SetActive(false);
        loadingIcon.SetActive(true);
        loadingStatuts.text = "...";
    }

    public void Local()
    {
        CustomNetworkManager.serverBindToIP = false;
        CustomNetworkManager.networkAddress = "localhost";
        CustomNetworkManager.networkPort = 7777;

        if (Input.GetKey(KeyCode.LeftShift))
            CustomNetworkManager.StartClient();
        else
            CustomNetworkManager.StartHost();

        Application.quitting += Quit;
    }

    public void OnlineButtons()
    {
        normalButtons.GetComponent<Animator>().Play("ButtonsLeft");
        StartCoroutine(FinishAnimation(true));
    }

    public void NormalButtons()
    {
        onlineButtons.GetComponent<Animator>().Play("OnlineButtonsRight");
        StartCoroutine(FinishAnimation(false));
    }

    IEnumerator FinishAnimation(bool _normalButtons)
    {
        yield return new WaitForSeconds(0.45f);
        if (_normalButtons)
        {
            normalButtons.SetActive(false);
            onlineButtons.SetActive(true);
            onlineButtons.GetComponent<Animator>().Play("OnlineButtonsLeft");
            yield return new WaitForSeconds(0.57f);
            onlineButtons.SetActive(false); //workaround
            onlineButtons.SetActive(true);  //workaround
        }
        if (!_normalButtons)
        {
            normalButtons.SetActive(true);
            onlineButtons.SetActive(false);
            normalButtons.GetComponent<Animator>().Play("ButtonsRight");
            yield return new WaitForSeconds(0.57f);
            normalButtons.SetActive(false); //workaround
            normalButtons.SetActive(true);  //workaround
        }
    }

    public void Quit()
    {
        PlayerPrefs.SetString("OpenFile", "");
        Application.Quit();
    }

}



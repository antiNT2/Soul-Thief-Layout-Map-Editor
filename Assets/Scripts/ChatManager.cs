using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{

    public static ChatManager instance;
    [SerializeField]
    GameObject messagesPanel;
    [SerializeField]
    Transform messagesParent;
    [SerializeField]
    InputField messageField;
    [SerializeField]
    GameObject messageObjectPrefab;

    AudioSource audioSource;
    public List<ChatMessage> receivedMessages = new List<ChatMessage>();


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            messageField.Select();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2) || GridManager.instance.canPaint == true)
            CloseMessagesPanel();

        if (messagesParent.childCount > 9)
            Destroy(messagesParent.GetChild(0).gameObject);
    }

    public void SendMessage()
    {
        if (messageField.text != "")
            Manager.localPlayerManager.CmdSendMessage(messageField.text, GetLocalUserName(),-1);
        messageField.text = "";
        //CloseMessagesPanel();
    }

    public void ReceiveMessage(string _content, string _userName)
    {
        ChatMessage received = new ChatMessage();
        received.username = _userName;
        received.messageContent = _content;
        receivedMessages.Add(received);

        if (_userName != GetLocalUserName() && _userName != "")
            audioSource.Play();

        OpenMessagesPanel();
        Text messageDisplay = Instantiate(messageObjectPrefab, messagesParent).GetComponentInChildren<Text>();
        if (_userName != "")
            _userName += ": ";
        messageDisplay.text = _userName + _content;
        messageField.Select();
    }

    public void OpenMessagesPanel()
    {
        messagesPanel.SetActive(true);
    }

    public void CloseMessagesPanel()
    {
        messagesPanel.SetActive(false);
    }

    public string GetLocalUserName()
    {
        if (Manager.localPlayerManager == null)
            return "";

        int thisPlayerID = int.Parse(Manager.localPlayerManager.netId.ToString());
        string colorHex = "FFFFFF";
        if (thisPlayerID == 1)
            colorHex = "26C3A8";
        if (thisPlayerID == 2)
            colorHex = ColorUtility.ToHtmlStringRGB(Color.black);
        if (thisPlayerID == 3)
            colorHex = ColorUtility.ToHtmlStringRGB(Color.green);
        if (thisPlayerID == 4)
            colorHex = ColorUtility.ToHtmlStringRGB(Color.yellow);
        if (thisPlayerID == 5)
            colorHex = ColorUtility.ToHtmlStringRGB(Color.cyan);
        if (thisPlayerID >= 6)
            colorHex = ColorUtility.ToHtmlStringRGB(Color.blue);

        return "<color=#" + colorHex + ">User " + thisPlayerID + "</color>";
    }
}

public class ChatMessage
{
    public string username;
    public string messageContent;
}

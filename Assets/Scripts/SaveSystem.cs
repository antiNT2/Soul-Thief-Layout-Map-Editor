using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using SFB;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public string workingDirectory = "";

    LayerManager layerManager;
    [SerializeField]
    GameObject panel;
    [SerializeField]
    Image closePanelButton;
    [SerializeField]
    Text currentFileName;

    bool menuOpen;

    public static SaveSystem instance;

    void Start()
    {
        layerManager = LayerManager.instance;
        instance = this;
        if (PlayerPrefs.GetString("OpenFile") != "" && PlayerPrefs.GetString("OpenFile") != "done")
        {
            Load(PlayerPrefs.GetString("OpenFile"));
            currentFileName.text = Path.GetFileName(PlayerPrefs.GetString("OpenFile"));
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            SaveButton();

        if(PlayerPrefs.GetString("OpenFile") != "done" && (PlayerPrefs.GetString("OpenFile") != ""))
        {
            string directory = PlayerPrefs.GetString("OpenFile");
            Load(directory);
            workingDirectory = directory;
        }
            
            
    }

    public void SaveAsButton()
    {
        var extension = new[] {
                new ExtensionFilter("Map Save", "msav" ),
            };
        string fileName = Path.GetFileNameWithoutExtension(currentFileName.text);
        Save(StandaloneFileBrowser.SaveFilePanel("Save File", Application.persistentDataPath, fileName, extension));
        CloseMenu();
    }

    public void SaveButton()
    {
        print(workingDirectory);

        if (workingDirectory != "")
            Save(workingDirectory);
        else
            SaveAsButton();
        CloseMenu();
    }

    public void NewButton()
    {
        foreach (Layer l in layerManager.layers)
        {
            foreach (Tile t in l.allTiles)
            {
                GridManager.instance.DestroyCell(l.layerID, t.gridPos, -1);
            }
        }
        workingDirectory = "";
        currentFileName.text = "New Map.msav";

        Manager.localPlayerManager.CmdClearLayers();
        CloseMenu();
    }

    public void LoadButton()
    {
        if (Manager.localPlayerManager.networkIdentity.isServer)
        {
            var extension = new[] {
                new ExtensionFilter("Map Save", "msav" ),
            };
            Load(StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, extension, false)[0]);
            CloseMenu();
        }
    }

    public void MainMenuButton()
    {
        FindObjectOfType<CustomNetworkManager>().StopHost();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void DropDownMenu()
    {
        menuOpen = !menuOpen;
        if (menuOpen)
        {
            panel.gameObject.SetActive(true);
            closePanelButton.rectTransform.localRotation = new Quaternion(0, 0, 0, 0);
        }
        else
        {
            closePanelButton.rectTransform.localRotation = new Quaternion(0, 0, 180, 0);
            panel.gameObject.SetActive(false);
        }
    }

    public void CloseMenu()
    {
        menuOpen = false;
        closePanelButton.rectTransform.localRotation = new Quaternion(0, 0, 180, 0);
        panel.gameObject.SetActive(false);

    }

    void Save(string destination)
    {
        if (destination == "" || destination == null)
            return;

        workingDirectory = destination;
        currentFileName.text = Path.GetFileName(workingDirectory);

        SaveData dataToSave = new SaveData();
        dataToSave.layers = layerManager.layers;
        dataToSave.cameraPos = Camera.main.transform.position;

        //string destination = Application.persistentDataPath + "/save.msav";
        FileStream file;

        if (File.Exists(destination))
            file = File.OpenWrite(destination);
        else
            file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, dataToSave);
        file.Close();

        print("DATA SAVED");
    }

    void Load(string destination)
    {
        if (destination == "" || destination == null)
            return;

        //string destination = Application.persistentDataPath + "/save.msav";
        FileStream file;

        if (File.Exists(destination))
            file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        SaveData data = (SaveData)bf.Deserialize(file);
        file.Close();

        foreach (Layer l in layerManager.layers)
        {
            foreach (Tile t in l.allTiles)
            {
                GridManager.instance.DestroyCell(l.layerID, t.gridPos, -1);
            }
        }
        layerManager.ClearLayers();
        layerManager.layers = data.layers;
        Camera.main.transform.position = data.cameraPos;
        Manager.localPlayerManager.CmdPlaceAllTiles(-1);
        //layerManager.layers = data.layers;
        print("DATA LOADED AT " + destination);

        workingDirectory = destination;
        currentFileName.text = Path.GetFileName(workingDirectory);
        print("Working directory = " + workingDirectory);
        PlayerPrefs.SetString("OpenFile", "done");
    }

}

[System.Serializable]
public class SaveData
{
    public SerializableVector2 cameraPos;
    public List<Layer> layers;
}

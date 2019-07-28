using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    [SerializeField]
    GameObject layerDisplayPrefab;
    [SerializeField]
    public GameObject layerDisplayParent;
    [SerializeField]
    public Grid layerTilemapsParent;
    [SerializeField]
    public Tilemap layerTilemapsPrefab;

    public List<Layer> layers = new List<Layer>();
    public int activeLayer = 0;

    public static LayerManager instance;

    
    private void Awake()
    {
        instance = this;
    }

    public void AddLayerButton()
    {
        /*Layer myLayer = new Layer();
        myLayer.layerName = "Layer " + layers.Count;
        AddLayer(myLayer);*/
        Manager.localPlayerManager.CmdCreateLayer(int.Parse(Manager.localPlayerManager.netId.ToString()));
        Manager.localPlayerManager.CmdSendMessage(ChatManager.instance.GetLocalUserName() + " added a layer.", "", -1);
    }

    public void DeleteSelectedLayerButton()
    {
        /*if (activeLayer != 0)
        {
            GetLayer(activeLayer).deleted = true;
        }
        activeLayer = 0;*/
        Manager.localPlayerManager.CmdDeleteSelectedLayer(activeLayer, int.Parse(Manager.localPlayerManager.netId.ToString()));
        if (activeLayer > 0)
            Manager.localPlayerManager.CmdSendMessage(ChatManager.instance.GetLocalUserName() + " deleted a layer.", "", -1);
    }

    public void EditNameSelectedLayerButton()
    {
        GetLayer(activeLayer).beginEditName = true;
    }

    public void SetLayerVisibility(int id, bool visible)
    {
        GetLayer(id).visible = visible;
        GetLayer(id).tilemap.GetComponent<TilemapRenderer>().enabled = visible;
    }

    public void SetLayerName(int id, string name)
    {
        GetLayer(id).layerName = name;
    }

    public void AddLayer(Layer layerToAdd, int undoPlayerID)
    {
        GameObject layerDisplay = Instantiate(layerDisplayPrefab, layerDisplayParent.transform);
        layerToAdd.layerDisplay = layerDisplay;
        if (layers.Count > 0)
            layerToAdd.layerID = layers[layers.Count - 1].layerID + 1;
        else
            layerToAdd.layerID = 0;
        layerDisplay.name = "LayerDislay " + layerToAdd.layerID;
        layerDisplay.GetComponent<LayerDisplay>().thisLayer = layerToAdd.layerID;
        layerToAdd.sortingOrder = layerToAdd.layerID;
        layers.Add(layerToAdd);
        if (layerToAdd.layerID != 0 && undoPlayerID == int.Parse(Manager.localPlayerManager.netId.ToString()))
            UndoActions.instance.AddDataToCurrentAction(ActionData.ActionType.CreateLayer, null, layerToAdd);

        SpawnLayerTilemap(layerToAdd);
    }

    public void AddLoadedLayers(Layer layerToAdd)
    {
        //Used by the save system
        GameObject layerDisplay = Instantiate(layerDisplayPrefab, layerDisplayParent.transform);
        layerDisplay.name = layerToAdd.layerName;
        layerDisplay.GetComponent<LayerDisplay>().thisLayer = layerToAdd.layerID;

        SpawnLayerTilemap(layerToAdd);
    }

    public void SpawnLayerTilemap(Layer layerToAdd)
    {
        Tilemap layerTilemap = Instantiate(layerTilemapsPrefab, layerTilemapsParent.transform);
        layerTilemap.gameObject.name = "Layer " + layerToAdd.layerID;
        layerTilemap.GetComponent<TilemapRenderer>().sortingOrder = layerToAdd.sortingOrder;
        layerToAdd.tilemap = layerTilemap;
    }

    public void ClearLayers()
    {
        for (int j = 0; j < layerDisplayParent.transform.childCount; j++)
        {
            Destroy(layerDisplayParent.transform.GetChild(j).gameObject);
            Destroy(layerTilemapsParent.transform.GetChild(j).gameObject);
        }
        layers = new List<Layer>();
        activeLayer = 0;
    }

    public Layer GetLayer(int id)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].layerID == id)
            {
                return layers[i];
            }
        }

        Debug.LogError("LAYER " + id + " NOT FOUND");
        return null;
    }
}

[System.Serializable]
public class Layer
{
    public int layerID;
    public string layerName;
    [System.NonSerialized]
    public GameObject layerDisplay;
    [System.NonSerialized]
    public Tilemap tilemap;
    public List<Tile> allTiles = new List<Tile>();

    public Tile GetTile(int id)
    {
        for (int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i].id == id)
                return allTiles[i];
        }

        return null;
    }

    public int sortingOrder = 0;
    public bool visible = true;
    public bool deleted = false;
    public bool beginEditName = false;
}

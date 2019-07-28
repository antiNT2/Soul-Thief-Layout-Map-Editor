using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.SimpleColorPicker.Scripts;
using System.Linq;

public class Manager : NetworkBehaviour
{
    [SerializeField]
    GameObject userCursor;
    Camera cam;
    Vector2 mousePos;
    [HideInInspector]
    public NetworkIdentity networkIdentity;
    public static Manager localPlayerManager;

    private void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        this.gameObject.name = networkIdentity.netId.ToString();

        if (networkIdentity.isLocalPlayer == false)
            this.enabled = false;
        else
        {
            localPlayerManager = this;

            cam = Camera.main;
            mousePos = new Vector2();
        }
    }

    private void Update()
    {
        if (!Application.isFocused)
            return;

        mousePos = Input.mousePosition;
        userCursor.transform.position = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        networkIdentity = GetComponent<NetworkIdentity>();
        CmdPlaceAllTiles(int.Parse(networkIdentity.netId.ToString()));
        CmdSendChatToNewUser(int.Parse(networkIdentity.netId.ToString()));
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
    }

    [Command]
    public void CmdPlaceAllTiles(int netId)
    {
        if (netId > 1 || netId == -1)
        {
            RpcClearLayersOnSelectedPlayer(netId);
            for (int i = 0; i < LayerManager.instance.layers.Count; i++)
            {
                if (LayerManager.instance.GetLayer(i).deleted == false)
                {
                    if (i == 0)
                    {
                        RpcRenameLayerSelectedPlayer(netId, i, LayerManager.instance.GetLayer(i).layerName);
                    }
                    else
                    {
                        RpcCreateLayerOnSelectedPlayer(netId, LayerManager.instance.GetLayer(i).layerName, netId);
                    }

                    for (int j = 0; j < LayerManager.instance.layers[i].allTiles.Count; j++)
                    {
                        Tile t = LayerManager.instance.layers[i].allTiles[j];
                        RpcPlaceTileOnSelectedPlayer((int)t.type, t.tileColor, t.gridPos, i, netId, t.rotationDegrees);
                    }
                }
            }
        }
    }

    public void OpenFilePlaceAllTiles(int netId)
    {
        if (netId > 1 || netId == -1)
        {
            //RpcClearLayersOnSelectedPlayer(netId);
            for (int i = 0; i < LayerManager.instance.layers.Count; i++)
            {
                if (LayerManager.instance.GetLayer(i).deleted == false)
                {
                    if (i == 0)
                    {
                        CmdRenameLayerSelectedPlayer(netId, i, LayerManager.instance.GetLayer(i).layerName);
                    }
                    else
                    {
                        CmdCreateLayerOnSelectedPlayer(netId, LayerManager.instance.GetLayer(i).layerName, netId);
                    }

                    for (int j = 0; j < LayerManager.instance.layers[i].allTiles.Count; j++)
                    {
                        Tile t = LayerManager.instance.layers[i].allTiles[j];
                        CmdPlaceTileOnSelectedPlayer((int)t.type, t.tileColor, t.gridPos, i, netId, t.rotationDegrees);
                    }
                }
            }
        }
    }

    #region Place Tile
    [Command]
    public void CmdPlaceTile(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer, int undoPlayerID)
    {
        RpcPlaceTile(_shapeID, _tileColor, _gridPos, _layer, undoPlayerID);
    }

    [ClientRpc]
    void RpcPlaceTile(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer, int undoPlayerID)
    {
        if (LayerManager.instance.GetLayer(_layer) == null)
        {
            LayerManager layerManager = LayerManager.instance;
            Layer myLayer = new Layer();
            myLayer.layerName = "Layer " + layerManager.layers.Count;
            layerManager.AddLayer(myLayer, undoPlayerID);
        }

        GridManager.instance.PlaceSelectedTileAtMousePos(_shapeID, _tileColor, _gridPos, _layer, undoPlayerID);
    }

    [Command]
    void CmdPlaceTileOnSelectedPlayer(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer, int netID, float rotationZ)
    {
        RpcPlaceTileOnSelectedPlayer(_shapeID, _tileColor, _gridPos, _layer, netID, rotationZ);
    }

    [ClientRpc]
    void RpcPlaceTileOnSelectedPlayer(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer, int netID, float rotationZ)
    {
        if (networkIdentity == null)
            networkIdentity = GetComponent<NetworkIdentity>();

        if (int.Parse(networkIdentity.netId.ToString()) == netID || netID == -1)
            GridManager.instance.PlaceSelectedTileAtMousePos(_shapeID, _tileColor, _gridPos, _layer, -1, rotationZ);
    }
    #endregion

    #region RotateTile
    [Command]
    public void CmdRotateTile(Vector2 _gridPos, int _layer, float degrees, int undoPlayerID)
    {
        RpcRotateTile(_gridPos, _layer,degrees, undoPlayerID);
    }

    [ClientRpc]
    void RpcRotateTile(Vector2 _gridPos, int _layer, float degrees, int undoPlayerID)
    {
        GridManager.instance.RotateTile(_gridPos, _layer, degrees, undoPlayerID);
        //GridManager.instance.PlaceSelectedTileAtMousePos(_shapeID, _tileColor, _gridPos, _layer, undoPlayerID);
    }
    #endregion

    #region Remove Tile
    [Command]
    public void CmdRemoveTile(int layerID, Vector2 cellToDestroy, int undoPlayerID)
    {
        RpcRemoveTile(layerID, cellToDestroy, undoPlayerID);
    }

    [ClientRpc]
    void RpcRemoveTile(int layerID, Vector2 cellToDestroy, int undoPlayerID)
    {
        GridManager.instance.DestroyCell(layerID, cellToDestroy, undoPlayerID);
    }
    #endregion

    #region Create Layer
    [Command]
    public void CmdCreateLayer(int undoPlayerID)
    {
        RpcCreateLayer(undoPlayerID);
    }

    [ClientRpc]
    void RpcCreateLayer(int undoPlayerID)
    {
        LayerManager layerManager = LayerManager.instance;
        Layer myLayer = new Layer();
        myLayer.layerName = "Layer " + layerManager.layers.Count;
        layerManager.AddLayer(myLayer, undoPlayerID);
    }

    [Command]
    public void CmdCreateLayerOnSelectedPlayer(int netID, string layerName, int undoPlayerID)
    {
        RpcCreateLayerOnSelectedPlayer(netID, layerName, undoPlayerID);
    }

    [ClientRpc]
    void RpcCreateLayerOnSelectedPlayer(int netID, string layerName, int undoPlayerID)
    {
        if (networkIdentity == null)
            networkIdentity = GetComponent<NetworkIdentity>();

        if (int.Parse(networkIdentity.netId.ToString()) == netID || netID == -1)
        {
            LayerManager layerManager = LayerManager.instance;
            Layer myLayer = new Layer();
            myLayer.layerName = layerName;
            layerManager.AddLayer(myLayer, undoPlayerID);
        }
    }
    #endregion

    #region Clear Layers
    [Command]
    public void CmdClearLayers()
    {
        RpcClearLayers();
    }

    [ClientRpc]
    void RpcClearLayers()
    {
        LayerManager.instance.ClearLayers();
        Layer myLayer = new Layer();
        myLayer.layerName = "Base";
        LayerManager.instance.AddLayer(myLayer, -1);

        SaveSystem.instance.workingDirectory = "";

        UndoActions.instance.actionsMade.Clear();
        UndoActions.instance.AddAction();

        //CmdAddAction();
        Camera.main.transform.position = new Vector3(100f, 100f, -10);
    }

    [ClientRpc]
    void RpcClearLayersOnSelectedPlayer(int netID)
    {
        if (networkIdentity == null)
            networkIdentity = GetComponent<NetworkIdentity>();

        if (int.Parse(networkIdentity.netId.ToString()) == netID || netID == -1)
        {
            LayerManager.instance.ClearLayers();
            Layer myLayer = new Layer();
            myLayer.layerName = "Base";
            LayerManager.instance.AddLayer(myLayer, -1);

            SaveSystem.instance.workingDirectory = "";
            Camera.main.transform.position = new Vector3(100f, 100f, -10);
        }
    }
    #endregion

    #region Delete/Undelete Layer
    [Command]
    public void CmdDeleteSelectedLayer(int activeLayer, int undoPlayerID)
    {
        RpcDeleteSelectedLayer(activeLayer, undoPlayerID);
    }

    [ClientRpc]
    void RpcDeleteSelectedLayer(int activeLayer, int undoPlayerID)
    {
        if (activeLayer != 0)
        {
            LayerManager.instance.GetLayer(activeLayer).deleted = true;
            Destroy(LayerManager.instance.GetLayer(activeLayer).tilemap.gameObject);
        }
        LayerManager.instance.activeLayer = 0;
        if (undoPlayerID == int.Parse(Manager.localPlayerManager.netId.ToString()))
            UndoActions.instance.AddDataToCurrentAction(ActionData.ActionType.DeleteLayer, null, LayerManager.instance.GetLayer(activeLayer));
    }

    [Command]
    public void CmdUnDeleteSelectedLayer(int layerID, int undoPlayerID)
    {
        RpcUnDeleteSelectedLayer(layerID, undoPlayerID);
    }

    [ClientRpc]
    void RpcUnDeleteSelectedLayer(int layerID, int undoPlayerID)
    {
        LayerManager.instance.GetLayer(layerID).deleted = false;
        LayerManager.instance.GetLayer(layerID).layerDisplay.SetActive(true);
        LayerManager.instance.SpawnLayerTilemap(LayerManager.instance.GetLayer(layerID));

        if (undoPlayerID == int.Parse(Manager.localPlayerManager.netId.ToString()))
            UndoActions.instance.AddDataToCurrentAction(ActionData.ActionType.CreateLayer, null, LayerManager.instance.GetLayer(layerID));
    }
    #endregion

    #region Rename Layer
    [Command]
    public void CmdRenameLayer(int id, string _name)
    {
        RpcRenameLayer(id, _name);
    }

    [ClientRpc]
    void RpcRenameLayer(int id, string _name)
    {
        LayerManager.instance.SetLayerName(id, _name);
    }

    [Command]
    public void CmdRenameLayerSelectedPlayer(int playerID, int id, string _name)
    {
        RpcRenameLayerSelectedPlayer(playerID, id, _name);
    }

    [ClientRpc]
    void RpcRenameLayerSelectedPlayer(int playerID, int id, string _name)
    {
        if (networkIdentity == null)
            networkIdentity = GetComponent<NetworkIdentity>();

        if (int.Parse(networkIdentity.netId.ToString()) == playerID || playerID == -1)
        {
            LayerManager.instance.SetLayerName(id, _name);
        }
    }
    #endregion

    #region Action
    /*[Command]
    public void CmdAddAction()
    {
        RpcAddAction();
    }
    [ClientRpc]
    void RpcAddAction()
    {
        UndoActions.instance.AddAction();
    }*/
    [Command]
    public void CmdLockAction()
    {
        RpcLockAction();
    }
    [ClientRpc]
    void RpcLockAction()
    {
        UndoActions.instance.LockAction();
    }
    #endregion

    #region Chat
    [Command]
    public void CmdSendMessage(string _content, string _userName, int specifiedUserID)
    {
        RpcReceiveMessage(_content, _userName, specifiedUserID);
    }
    [ClientRpc]
    void RpcReceiveMessage(string _content, string _userName, int specifiedUserID)
    {
        if (specifiedUserID == -1 || specifiedUserID == int.Parse(Manager.localPlayerManager.netId.ToString()))
            ChatManager.instance.ReceiveMessage(_content, _userName);
    }
    [Command]
    public void CmdSendChatToNewUser(int specifiedUserID)
    {
        foreach (ChatMessage m in ChatManager.instance.receivedMessages)
        {
            RpcReceiveMessage(m.messageContent, m.username, specifiedUserID);
        }
    }
    #endregion
}


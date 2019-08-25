using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoActions : MonoBehaviour
{
    public static UndoActions instance;

    public List<Action> actionsMade = new List<Action>();
    //public List<SaveData> snapshots = new List<SaveData>();
    Dictionary<int, ActionData> dataToAddAtId = new Dictionary<int, ActionData>();

    private void Awake()
    {
        instance = this;
        AddAction();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //AddAction();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            LockAction();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            if (SelectTiles.instance.HasSelectedTiles() == false)
                Undo();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            if (SelectTiles.instance.HasSelectedTiles() == false)
                Redo();
        }

        for (int i = 0; i < actionsMade.Count; i++)
        {
            if (actionsMade[i].locked == true && actionsMade[i].actionData.Count == 0 && i > 0)
                actionsMade.RemoveAt(i); //Prevents from having empty actions
            if (i == 0)
                actionsMade[i].locked = true;
        }
    }

    public void AddAction()
    {
        if (GetLastActualAction() >= actionsMade.Count - 1)
        {
            actionsMade.Add(new Action());
            foreach (KeyValuePair<int, ActionData> item in dataToAddAtId)
            {
                if (item.Key == actionsMade.Count - 1)
                    actionsMade[item.Key].actionData.Add(item.Value);
            }
        }
        else
        {
            actionsMade[GetLastActualAction() + 1] = new Action();
            if (actionsMade.Count - 1 - GetLastActualAction() + 2 < actionsMade.Count)
                RemoveActionsFromIndex(GetLastActualAction() + 1);
            foreach (KeyValuePair<int, ActionData> item in dataToAddAtId)
            {
                if (item.Key == GetLastActualAction())
                    actionsMade[item.Key].actionData.Add(item.Value);
            }
        }
    }

    public void LockAction()
    {
        if (actionsMade.Count == 0)
            AddAction();

        actionsMade[actionsMade.Count - 1].locked = true;
    }

    void RemoveActionsFromIndex(int index)
    {
        for (int i = index; i < actionsMade.Count; i++)
        {
            actionsMade.RemoveAt(i);
        }
    }

    public void AddDataToCurrentAction(ActionData.ActionType type, Tile tile = null, Layer layer = null)
    {
        ActionData dataToAdd = new ActionData();
        dataToAdd.actionsMadeWithOneClick = type;
        dataToAdd.layersEdited = layer;
        dataToAdd.tilesEdited = tile;

        if (GetLastActualAction() > 0 && actionsMade[GetLastActualAction()].locked == false)
        {
            if (DataAlreadyExistInAction(GetLastActualAction(), type, tile, layer) == false)
            {
                // print("ADD DATA TO ACTION " + GetLastActualAction() + " (" + type.ToString() + ")");

                actionsMade[GetLastActualAction()].actionData.Add(dataToAdd);
            }
        }
        else if (actionsMade[GetLastActualAction()].locked == true && type == ActionData.ActionType.RemoveTile && dataToAddAtId.ContainsKey(GetLastActualAction() + 1) == false)
        {
            print("UnBlocked " + type.ToString());
            dataToAddAtId.Add(GetLastActualAction() + 1, dataToAdd);
        }
        else if (type == ActionData.ActionType.CreateLayer || type == ActionData.ActionType.DeleteLayer)
        {
            actionsMade[GetLastActualAction()].locked = true;
            AddAction();
            print("ADD DATA TO ACTION " + GetLastActualAction() + " (" + type.ToString() + ")");
            actionsMade[GetLastActualAction()].actionData.Add(dataToAdd);
        }
        else
        {
            print("BLOCKED " + type.ToString() + " last actual action is " + GetLastActualAction());
        }

    }

    bool DataAlreadyExistInAction(int id, ActionData.ActionType type, Tile tile = null, Layer layer = null)
    {
        if (id >= actionsMade.Count)
            return false;

        if (type == ActionData.ActionType.RemoveTile)
            type = ActionData.ActionType.PlaceTile; //bugfix

        for (int i = 0; i < actionsMade[id].actionData.Count; i++)
        {
            if (actionsMade[id].actionData[i].actionsMadeWithOneClick == type)
            {
                if (tile != null)
                    if (actionsMade[id].actionData[i].tilesEdited.gridPos == tile.gridPos)
                    {
                        return true;
                    }
                if (layer != null)
                    if (actionsMade[id].actionData[i].layersEdited.layerName == layer.layerName)
                    {
                        return true;
                    }
            }
        }

        return false;
    }

    int GetLastActualAction()
    {
        for (int i = actionsMade.Count - 1; i > 0; i--)
        {
            if (actionsMade[i].unDone == false)
            {
                return i;
            }
        }
        return 0;
    }

    void Undo()
    {
        if (actionsMade.Count == 0 || GetLastActualAction() == 0)
            return;

        //print("Undo action " + GetLastActualAction());
        for (int i = 0; i < actionsMade[GetLastActualAction()].actionData.Count; i++)
        {
            ActionData thisAction = actionsMade[GetLastActualAction()].actionData[i];
            ActionData.ActionType type = actionsMade[GetLastActualAction()].actionData[i].actionsMadeWithOneClick;

            if (type == ActionData.ActionType.CreateLayer)
            {
                Manager.localPlayerManager.CmdDeleteSelectedLayer(thisAction.layersEdited.layerID, -1);
            }
            if (type == ActionData.ActionType.DeleteLayer)
            {
                Manager.localPlayerManager.CmdUnDeleteSelectedLayer(thisAction.layersEdited.layerID, -1);
            }
            if (type == ActionData.ActionType.PlaceTile)
            {
                Manager.localPlayerManager.CmdRemoveTile(thisAction.tilesEdited.layerID, thisAction.tilesEdited.gridPos, -1);
            }
            if (type == ActionData.ActionType.RemoveTile)
            {
                Manager.localPlayerManager.CmdPlaceTile((int)thisAction.tilesEdited.type, thisAction.tilesEdited.tileColor, thisAction.tilesEdited.gridPos, thisAction.tilesEdited.layerID, -1, 0f);
            }

        }
        for (int i = 0; i < actionsMade[GetLastActualAction()].actionData.Count; i++)
        {
            ActionData thisAction = actionsMade[GetLastActualAction()].actionData[i];
            ActionData.ActionType type = actionsMade[GetLastActualAction()].actionData[i].actionsMadeWithOneClick;

            if (type == ActionData.ActionType.RemoveTile)
            {
                Manager.localPlayerManager.CmdPlaceTile((int)thisAction.tilesEdited.type, thisAction.tilesEdited.tileColor, thisAction.tilesEdited.gridPos, thisAction.tilesEdited.layerID, -1, 0f);
            }

        }

        actionsMade[GetLastActualAction()].unDone = true;
    }

    int GetLastUndoneAction()
    {
        for (int i = 0; i < actionsMade.Count; i++)
        {
            if (actionsMade[i].unDone == true)
            {
                return i;
            }
        }
        return -1;
    }

    void Redo()
    {
        if (actionsMade.Count == 0 || GetLastUndoneAction() == -1)
            return;

        for (int i = 0; i < actionsMade[GetLastUndoneAction()].actionData.Count; i++)
        {
            Action thisAction = actionsMade[GetLastUndoneAction()];
            ActionData.ActionType type = actionsMade[GetLastUndoneAction()].actionData[i].actionsMadeWithOneClick;

            if (type == ActionData.ActionType.DeleteLayer)
            {
                Manager.localPlayerManager.CmdDeleteSelectedLayer(thisAction.actionData[i].layersEdited.layerID, -1);
            }
            if (type == ActionData.ActionType.CreateLayer)
            {
                Manager.localPlayerManager.CmdUnDeleteSelectedLayer(thisAction.actionData[i].layersEdited.layerID, -1);
            }
            if (type == ActionData.ActionType.RemoveTile)
            {
                Manager.localPlayerManager.CmdRemoveTile(thisAction.actionData[i].tilesEdited.layerID, thisAction.actionData[i].tilesEdited.gridPos, -1);
            }
            if (type == ActionData.ActionType.PlaceTile)
            {
                Manager.localPlayerManager.CmdPlaceTile((int)thisAction.actionData[i].tilesEdited.type, thisAction.actionData[i].tilesEdited.tileColor, thisAction.actionData[i].tilesEdited.gridPos, thisAction.actionData[i].tilesEdited.layerID, -1, 0f);
            }
        }

        /*Manager.localPlayerManager.CmdClearLayers();
        LayerManager.instance.layers = actionsMade[GetLastUndoneAction()].snapshot.layers;
        Manager.localPlayerManager.OpenFilePlaceAllTiles(-1);*/

        actionsMade[GetLastUndoneAction()].unDone = false;
    }

    public void FuseLastTwoActions()
    {
        if (actionsMade.Count < 3)
            return;

        Action last = actionsMade[actionsMade.Count - 1];
        Action beforeLast = actionsMade[actionsMade.Count - 2];

        for (int i = 0; i < beforeLast.actionData.Count; i++)
        {
            last.actionData.Add(beforeLast.actionData[i]);
        }
        actionsMade.Remove(beforeLast);
    }
}

[System.Serializable]
public class Action
{
    public List<ActionData> actionData = new List<ActionData>();

    public bool unDone = false;
    public bool locked = false;
}

[System.Serializable]
public class ActionData
{
    public enum ActionType
    {
        None,
        PlaceTile,
        RemoveTile,
        CreateLayer,
        DeleteLayer
    }

    public ActionType actionsMadeWithOneClick;
    public Tile tilesEdited;
    public Layer layersEdited;
}

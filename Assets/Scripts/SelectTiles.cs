using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SelectTiles : MonoBehaviour
{
    Camera cam;
    GameObject spawnedRectangle;
    SpriteRenderer rectangleSprite;

    private Vector2 _anchorPoint;
    public static SelectTiles instance;

    [SerializeField]
    List<Tile> selectedTiles = new List<Tile>();
    UnityEngine.Tilemaps.Tilemap selectionTileMap;

    Vector2 mousePos;
    Vector2 lastMousePos;
    bool fuseLastActions;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (ToolSelection.instance.currentTool == ToolSelection.ToolType.SelectionRectangle)
        {
            if (spawnedRectangle == null)
            {
                spawnedRectangle = Manager.localPlayerManager.userSelectionRectangle;
                rectangleSprite = spawnedRectangle.GetComponent<SpriteRenderer>();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsMouseOnSelectedTiles() == false)
                {
                    LeaveSelection();
                }
            }
            else if ((Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1)) && spawnedRectangle != null) //drag
            {
                if (spawnedRectangle.activeSelf == true)
                {
                    rectangleSprite.size = (GetRectFromSelection().size);
                    spawnedRectangle.transform.position = GetRectFromSelection().position;
                    Manager.localPlayerManager.CmdSetRectangleSize(int.Parse(Manager.localPlayerManager.netId.ToString()), GetRectFromSelection().size);
                }
                else if (selectedTiles.Count > 0)
                {
                    DragSelection();
                }
            }
        }
        if ((Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1))) //drop
        {
            if (spawnedRectangle != null)
            {
                if (rectangleSprite.size.x > 0.1f && spawnedRectangle.activeSelf == true)
                {
                    if (Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        Select(GetRectFromSelection(), true);
                    }
                    else if (Input.GetKeyUp(KeyCode.Mouse1))
                        Select(GetRectFromSelection(), false);
                }
                spawnedRectangle.SetActive(false);
                Manager.localPlayerManager.CmdDisableRectangle(int.Parse(Manager.localPlayerManager.netId.ToString()));
            }
        }
        SetAllRectangleSizes();
        /*if (Input.GetKeyDown(KeyCode.Q))
            UndoActions.instance.FuseLastTwoActions();*/
    }

    IEnumerator UndoWorkaround()
    {
        yield return new WaitForSecondsRealtime(1f);
        UndoActions.instance.FuseLastTwoActions();
    }

    private void LateUpdate()
    {
        lastMousePos = Input.mousePosition;
    }

    private Rect GetRectFromSelection()
    {
        Vector3 realMousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        var rect = new Rect(
            _anchorPoint.x,
            _anchorPoint.y,
            realMousePos.x - _anchorPoint.x,
            realMousePos.y - _anchorPoint.y
        );

        // Normalize bounds of rectangle (don't want negative area).
        if (rect.width < 0)
        {
            rect.x += rect.width;
            rect.width = -rect.width;
        }
        if (rect.height < 0)
        {
            rect.y += rect.height;
            rect.height = -rect.height;
        }

        return rect;
    }

    void Select(Rect selectionRect, bool destroyOriginalCell)
    {
        fuseLastActions = destroyOriginalCell;
        if (destroyOriginalCell)
            UndoActions.instance.AddAction();

        for (int i = 0; i < ((int)selectionRect.width + 1); i++)
        {
            for (int j = 0; j < ((int)selectionRect.height + 1); j++)
            {
                Tile tileToAdd = GridManager.instance.GetCellByGridPos(LayerManager.instance.activeLayer, new Vector2(((int)selectionRect.x + i), ((int)selectionRect.y + j)));
                if (tileToAdd != null)
                {
                    selectedTiles.Add(tileToAdd.Clone());
                    TransferTileToSelectionTilemap(selectedTiles[selectedTiles.Count - 1], -1);
                    Manager.localPlayerManager.CmdTransferTileToSelectionTilemap((int)selectedTiles[selectedTiles.Count - 1].type, selectedTiles[selectedTiles.Count - 1].tileColor, selectedTiles[selectedTiles.Count - 1].gridPos, selectedTiles[selectedTiles.Count - 1].layerID, int.Parse(Manager.localPlayerManager.netId.ToString()), selectedTiles[selectedTiles.Count - 1].rotationDegrees);

                    if (destroyOriginalCell)
                    {
                        GridManager.instance.DestroyCell(LayerManager.instance.activeLayer, tileToAdd.gridPos, -2);
                        Manager.localPlayerManager.CmdRemoveTile(LayerManager.instance.activeLayer, tileToAdd.gridPos, int.Parse(Manager.localPlayerManager.netId.ToString()));
                    }
                }
            }
        }
    }

    public void TransferTileToSelectionTilemap(Tile tileToTransfer, int selectionTilemapId)
    {
        if (selectionTilemapId != int.Parse(Manager.localPlayerManager.netId.ToString())) //prevents doing it to the local player
        {
            if (selectionTileMap == null)
                selectionTileMap = Manager.localPlayerManager.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();

            UnityEngine.Tilemaps.Tilemap selectionTM;
            if (selectionTilemapId != -1)
                selectionTM = Manager.localPlayerManager.FindPlayerById(selectionTilemapId).GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
            else
                selectionTM = selectionTileMap;


            UnityEngine.Tilemaps.Tile newTile = GridManager.instance.tilemapTiles[(int)tileToTransfer.type];
            newTile.color = tileToTransfer.tileColor;

            selectionTM.SetTile(tileToTransfer.gridPos, newTile);
            Quaternion rotation = Quaternion.identity;
            rotation.eulerAngles = Vector3.forward * tileToTransfer.rotationDegrees;
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            selectionTM.SetTransformMatrix(tileToTransfer.gridPos, matrix);
        }
    }

    void PlaceTilesToRealTilemap()
    {
        if (fuseLastActions)
            UndoActions.instance.FuseLastTwoActions();
        UnityEngine.Tilemaps.Tilemap selectionTM = selectionTileMap;

        for (int i = 0; i < selectedTiles.Count; i++)
        {
            Tile tileToDeselect = selectedTiles[i];
            if (tileToDeselect != null && tileToDeselect.destroyed == false)
            {
                Vector2 newPos = new Vector2(tileToDeselect.gridPos.x + Mathf.RoundToInt(selectionTM.transform.position.x), tileToDeselect.gridPos.y + Mathf.RoundToInt(selectionTM.transform.position.y));
                Manager.localPlayerManager.CmdPlaceTile((int)tileToDeselect.type, tileToDeselect.tileColor, newPos, tileToDeselect.layerID, int.Parse(Manager.localPlayerManager.netId.ToString()), tileToDeselect.rotationDegrees);
                GridManager.instance.PlaceSelectedTileAtMousePos((int)tileToDeselect.type, tileToDeselect.tileColor, newPos, tileToDeselect.layerID, -2, tileToDeselect.rotationDegrees);
            }
        }
        selectedTiles.Clear();
        ClearSelectionTilemap(-1);
        Manager.localPlayerManager.CmdClearSelectionTilemap(int.Parse(Manager.localPlayerManager.netId.ToString()));
        fuseLastActions = false;
    }

    void DragSelection()
    {
        mousePos = Input.mousePosition;
        float scrollSpeed = 0.5f;
        float minimumScroll = 0.0005f;
        float mouseDeltaX = Mathf.Abs(mousePos.x - lastMousePos.x) + 1f;
        float mouseDeltaY = Mathf.Abs(mousePos.y - lastMousePos.y) + 1f;

        scrollSpeed *= cam.orthographicSize;

        /*if (mousePos.x < lastMousePos.x - minimumScroll)
        {
            selectionTileMap.transform.position = Vector3.Lerp(selectionTileMap.transform.position, selectionTileMap.transform.position + Vector3.left, Time.deltaTime * scrollSpeed * mouseDeltaX);
        }
        if (mousePos.x > lastMousePos.x + minimumScroll)
        {
            selectionTileMap.transform.position = Vector3.Lerp(selectionTileMap.transform.position, selectionTileMap.transform.position + Vector3.right, Time.deltaTime * scrollSpeed * mouseDeltaX);
        }
        if (mousePos.y < lastMousePos.y - (minimumScroll * 0.6f))
        {
            selectionTileMap.transform.position = Vector3.Lerp(selectionTileMap.transform.position, selectionTileMap.transform.position + Vector3.down, Time.deltaTime * scrollSpeed * mouseDeltaY);
        }
        if (mousePos.y > lastMousePos.y + (minimumScroll * 0.6f))
        {
            selectionTileMap.transform.position = Vector3.Lerp(selectionTileMap.transform.position, selectionTileMap.transform.position + Vector3.up, Time.deltaTime * scrollSpeed * mouseDeltaY);
        }*/

        selectionTileMap.transform.position += (cam.ScreenToWorldPoint(mousePos) - cam.ScreenToWorldPoint(lastMousePos));
    }

    bool IsMouseOnSelectedTiles()
    {
        Vector2 mouseGridPos = GridManager.instance.Vector2ToGrid(cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane)));
        for (int i = 0; i < selectedTiles.Count; i++)
        {
            Vector2 posToCheck = new Vector2(selectedTiles[i].gridPos.x + Mathf.RoundToInt(selectionTileMap.transform.position.x), selectedTiles[i].gridPos.y + Mathf.RoundToInt(selectionTileMap.transform.position.y));
            Vector2 difference = new Vector2(Mathf.Abs(mouseGridPos.x) - Mathf.Abs(posToCheck.x), Mathf.Abs(mouseGridPos.y) - Mathf.Abs(posToCheck.y));

            if (Mathf.Abs(difference.x) <= 1 && Mathf.Abs(difference.y) <= 1)
                return true;
        }
        return false;
    }

    void LeaveSelection()
    {
        print("STOP SELECTION");
        PlaceTilesToRealTilemap();
        spawnedRectangle.transform.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        _anchorPoint = spawnedRectangle.transform.position;
        rectangleSprite.size = Vector2.zero;
        spawnedRectangle.SetActive(true);
        Manager.localPlayerManager.CmdEnableRectangle(int.Parse(Manager.localPlayerManager.netId.ToString()));
    }

    public bool HasSelectedTiles()
    {
        if (selectedTiles.Count > 0)
            return true;
        else
            return false;
    }

    public void RotateAllSelectedTiles(float degrees)
    {
        for (int i = 0; i < selectedTiles.Count; i++)
        {
            Quaternion oldRotation = selectionTileMap.GetTransformMatrix(selectedTiles[i].gridPos).rotation;
            oldRotation.eulerAngles += Vector3.forward * degrees;
            selectedTiles[i].rotationDegrees = oldRotation.eulerAngles.z;
            RotateTileOnSelectionTilemap(selectedTiles[i].gridPos, degrees, -1);
            Manager.localPlayerManager.CmdRotateTileOnSelectionTilemap(selectedTiles[i].gridPos, degrees, int.Parse(Manager.localPlayerManager.netId.ToString()));
        }
    }

    public void DeleteAllSelectedTiles()
    {
        for (int i = 0; i < selectedTiles.Count; i++)
        {
            selectedTiles[i].destroyed = true;
            DeleteTileOnSelectionTilemap(selectedTiles[i].gridPos, -1);
            Manager.localPlayerManager.CmdDeleteTileOnSelectionTilemap(selectedTiles[i].gridPos, int.Parse(Manager.localPlayerManager.netId.ToString()));
        }
        PlaceTilesToRealTilemap();
    }

    #region Networking
    public void EnableRectangle(int rectangleID)
    {
        if (int.Parse(Manager.localPlayerManager.netId.ToString()) != rectangleID) //prevents placing the rectangle on the local player
        {
            GameObject rectangle = Manager.localPlayerManager.FindPlayerById(rectangleID).transform.GetChild(1).gameObject;
            Manager.localPlayerManager.CmdSetRectangleSize(rectangleID, Vector2.zero);
            rectangle.SetActive(true);
        }
    }

    void SetAllRectangleSizes()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            SetRectangleSize(int.Parse(players[i].GetComponentInParent<NetworkIdentity>().netId.ToString()));
        }
    }

    public void SetRectangleSize(int rectangleID)
    {
        if (int.Parse(Manager.localPlayerManager.netId.ToString()) != rectangleID) //prevents placing the rectangle on the local player
        {
            GameObject rectangle = Manager.localPlayerManager.FindPlayerById(rectangleID).transform.GetChild(1).gameObject;
            //if (rectangle.activeSelf == true)
            rectangle.GetComponent<SpriteRenderer>().size = Manager.localPlayerManager.FindPlayerById(rectangleID).GetComponent<Manager>().selectionRectangleSize;
        }
    }

    public void DisableRectangle(int rectangleID)
    {
        if (int.Parse(Manager.localPlayerManager.netId.ToString()) != rectangleID) //prevents removing the rectangle on the local player
        {
            GameObject rectangle = Manager.localPlayerManager.FindPlayerById(rectangleID).transform.GetChild(1).gameObject;
            Manager.localPlayerManager.CmdSetRectangleSize(rectangleID, Vector2.zero);
            rectangle.SetActive(false);
        }
    }

    public void ClearSelectionTilemap(int selectionTilemapId)
    {
        if (selectionTilemapId != int.Parse(Manager.localPlayerManager.netId.ToString())) //prevents doing it to the local player
        {
            UnityEngine.Tilemaps.Tilemap selectionTM;
            if (selectionTilemapId != -1)
                selectionTM = Manager.localPlayerManager.FindPlayerById(selectionTilemapId).GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
            else
                selectionTM = selectionTileMap;

            if (selectionTM != null)
            {
                selectionTM.transform.position = Vector3.zero;
                selectionTM.ClearAllTiles();
            }
        }
    }

    public void RotateTileOnSelectionTilemap(Vector2 _gridPos, float degrees, int selectionTilemapId)
    {
        if (selectionTilemapId != int.Parse(Manager.localPlayerManager.netId.ToString())) //prevents doing it to the local player
        {
            if (selectionTileMap == null)
                selectionTileMap = Manager.localPlayerManager.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();

            UnityEngine.Tilemaps.Tilemap selectionTM;
            if (selectionTilemapId != -1)
                selectionTM = Manager.localPlayerManager.FindPlayerById(selectionTilemapId).GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
            else
                selectionTM = selectionTileMap;


            Vector3Int gridPos = new Vector3Int((int)_gridPos.x, (int)_gridPos.y, -10);
            Quaternion oldRotation = selectionTM.GetTransformMatrix(gridPos).rotation;
            oldRotation.eulerAngles += Vector3.forward * degrees;
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, oldRotation, Vector3.one);
            selectionTM.SetTransformMatrix(gridPos, matrix);
        }
    }

    public void DeleteTileOnSelectionTilemap(Vector2 _gridPos, int selectionTilemapId)
    {
        if (selectionTilemapId != int.Parse(Manager.localPlayerManager.netId.ToString())) //prevents doing it to the local player
        {
            if (selectionTileMap == null)
                selectionTileMap = Manager.localPlayerManager.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();

            UnityEngine.Tilemaps.Tilemap selectionTM;
            if (selectionTilemapId != -1)
                selectionTM = Manager.localPlayerManager.FindPlayerById(selectionTilemapId).GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
            else
                selectionTM = selectionTileMap;


            Vector3Int gridPos = new Vector3Int((int)_gridPos.x, (int)_gridPos.y, -10);
            selectionTM.SetTile(gridPos, null);
        }
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assets.SimpleColorPicker.Scripts;
using System.Runtime.Serialization;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public float xOffset;
    public float yOffset;
    public int shapeID = 0;

    /*[SerializeField]
    GameObject tileParent;
    public GameObject[] tiles;*/

    [Header("----------")]
    [SerializeField]
    public UnityEngine.Tilemaps.Tile[] tilemapTiles;

    Camera cam;
    Vector2 mousePos;
    Vector2 lastMousePos;

    public bool canPaint = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        mousePos = new Vector2();

        if (SaveSystem.instance.workingDirectory == "")
        {
            Layer firstLayer = new Layer();
            firstLayer.layerName = "Base";
            LayerManager.instance.AddLayer(firstLayer, -1);
        }
    }

    private void Update()
    {
        #region Placing Tiles and scrolling
        mousePos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Manager.localPlayerManager.CmdAddAction();
            UndoActions.instance.AddAction();
        }

        if (Input.GetKey(KeyCode.Mouse0) && canPaint && Input.GetKey(KeyCode.LeftAlt) == false && ToolSelection.instance.currentTool == ToolSelection.ToolType.Paint)
        {
            Cursor.visible = false;
            if (IsSameTilePresentAtGridPos(shapeID, ColorPicker.Instance.Color, Vector2ToGrid(cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane))), LayerManager.instance.activeLayer) == false)
            {
                Manager.localPlayerManager.CmdPlaceTile(shapeID, ColorPicker.Instance.Color, Vector2ToGrid(cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane))), LayerManager.instance.activeLayer, int.Parse(Manager.localPlayerManager.netId.ToString()), 0f);
                GridManager.instance.PlaceSelectedTileAtMousePos(shapeID, ColorPicker.Instance.Color, Vector2ToGrid(cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane))), LayerManager.instance.activeLayer, -2);
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cam.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * -4f;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 2f, 40f);
        }
        if (Input.GetKey(KeyCode.Mouse2) || (Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.LeftAlt)))
        {
            float scrollSpeed = 4f;
            float minimumScroll = 0.0005f;
            float mouseDeltaX = Mathf.Abs(mousePos.x - lastMousePos.x) + 1f;
            float mouseDeltaY = Mathf.Abs(mousePos.y - lastMousePos.y) + 1f;

            if (Input.GetKey(KeyCode.LeftControl))
                scrollSpeed = 8f;
            if (Input.GetKey(KeyCode.LeftShift))
                scrollSpeed = 2f;

            if (mousePos.x < lastMousePos.x - minimumScroll)
            {
                //cam.transform.Translate(Vector2.right * Time.deltaTime * scrollSpeed);
                cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + Vector3.right * scrollSpeed, Time.deltaTime * scrollSpeed * mouseDeltaX);
            }
            if (mousePos.x > lastMousePos.x + minimumScroll)
            {
                //cam.transform.Translate(Vector2.left * Time.deltaTime * scrollSpeed);
                cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + Vector3.left * scrollSpeed, Time.deltaTime * scrollSpeed * mouseDeltaX);
            }
            if (mousePos.y < lastMousePos.y - (minimumScroll * 0.6f))
            {
                //cam.transform.Translate(Vector2.up * Time.deltaTime * scrollSpeed);
                cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + Vector3.up * scrollSpeed, Time.deltaTime * scrollSpeed * mouseDeltaY);
            }
            if (mousePos.y > lastMousePos.y + (minimumScroll * 0.6f))
            {
                //cam.transform.Translate(Vector2.down * Time.deltaTime * scrollSpeed);
                cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + Vector3.down * scrollSpeed, Time.deltaTime * scrollSpeed * mouseDeltaY);
            }
        }

        Vector2 mouse = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (GetCellByGridPos(LayerManager.instance.activeLayer, Vector2ToGrid(mouse)) != null && LayerManager.instance.GetLayer(LayerManager.instance.activeLayer).visible == true)
                    ShapePicker.instance.SelectShape((int)GetCellByGridPos(LayerManager.instance.activeLayer, Vector2ToGrid(mouse)).type);
            }
            else
            {
                Color pickedColor = new Color();
                if (GetCellByGridPos(LayerManager.instance.activeLayer, Vector2ToGrid(mouse)) != null && LayerManager.instance.GetLayer(LayerManager.instance.activeLayer).visible == true)
                    pickedColor = GetCellByGridPos(LayerManager.instance.activeLayer, Vector2ToGrid(mouse)).tileColor;
                ColorPicker.Instance.SetColor(pickedColor, sliders: false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (ToolSelection.instance.currentTool == ToolSelection.ToolType.SelectionRectangle && SelectTiles.instance.HasSelectedTiles())
                SelectTiles.instance.RotateAllSelectedTiles(-45f);
            else
                RotateSelectedTileAtMousePos(Vector2ToGrid(mouse), -45f);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (ToolSelection.instance.currentTool == ToolSelection.ToolType.SelectionRectangle && SelectTiles.instance.HasSelectedTiles())
                SelectTiles.instance.RotateAllSelectedTiles(45f);
            else
                RotateSelectedTileAtMousePos(Vector2ToGrid(mouse), 45f);
        }
        #endregion

        /*if (Input.GetKeyDown(KeyCode.Q))
            Debug.Log(Vector2ToGrid(cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane))).ToString());*/
    }

    private void LateUpdate()
    {
        lastMousePos = mousePos;
    }

    public void PaintButtonClick()
    {
        canPaint = true;
    }
    public void PaintButtonUnclick()
    {
        canPaint = false;
    }

    void RotateSelectedTileAtMousePos(Vector2 _gridPos, float degrees)
    {
        Manager.localPlayerManager.CmdRotateTile(_gridPos, LayerManager.instance.activeLayer, degrees, int.Parse(Manager.localPlayerManager.netId.ToString()));
        RotateTile(_gridPos, LayerManager.instance.activeLayer, degrees, -2);
    }

    public void PlaceSelectedTileAtMousePos(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer, int undoPlayerID = -1, float rotationZ = 0f)
    {
        if (int.Parse(Manager.localPlayerManager.netId.ToString()) != undoPlayerID) //prevents placing the tile on the local player
        {
            Tile tileToPlace = new Tile();
            tileToPlace.type = (Tile.TileType)_shapeID;
            tileToPlace.tileColor = _tileColor;
            Vector2 gridPos = _gridPos;

            PlaceTile(tileToPlace, gridPos, _layer, undoPlayerID);
            RotateTile(gridPos, _layer, rotationZ);
        }
    }

    public void RotateTile(Vector2 _gridPos, int _layer, float degrees, float undoPlayerID = -1)
    {
        if (int.Parse(Manager.localPlayerManager.netId.ToString()) != undoPlayerID) //prevents placing the tile on the local player
        {
            if (IsCellEmpty(_layer, _gridPos))
                return;

            Vector3Int gridPos = new Vector3Int((int)_gridPos.x, (int)_gridPos.y, -10);
            Quaternion oldRotation = LayerManager.instance.GetLayer(_layer).tilemap.GetTransformMatrix(gridPos).rotation;
            oldRotation.eulerAngles += Vector3.forward * degrees;

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, oldRotation, Vector3.one);
            LayerManager.instance.GetLayer(_layer).tilemap.SetTransformMatrix(gridPos, matrix);
            GetCellByGridPos(_layer, _gridPos).rotationDegrees = oldRotation.eulerAngles.z;
        }
    }

    Vector2 GridPosToVector2(int x, int y)
    {
        Vector2 pos = new Vector2();

        pos.x = xOffset + x;
        pos.y = yOffset + y;

        return pos;
    }

    bool IsSameTilePresentAtGridPos(int _shapeID, Color _tileColor, Vector2 _gridPos, int _layer)
    {
        for (int i = 0; i < LayerManager.instance.layers[_layer].allTiles.Count; i++)
        {
            if (LayerManager.instance.GetLayer(_layer).allTiles[i].gridPos == _gridPos)
            {
                Tile tileToCheck = LayerManager.instance.GetLayer(_layer).allTiles[i];
                if (tileToCheck.destroyed == false && tileToCheck.tileColor == _tileColor && tileToCheck.type == (Tile.TileType)_shapeID)
                    return true;
            }
        }

        return false;
    }

    public Vector2 Vector2ToGrid(Vector2 vector)
    {
        Vector2 pos = new Vector2();
        //int xPos = Mathf.RoundToInt(vector.x - 0.12f);
        int xPos = (int)vector.x;
        //int yPos = Mathf.RoundToInt(vector.y);
        int yPos = (int)vector.y;

        #region Bugfix
        if (yPos < 0)
            yPos -= 1;
        if (xPos < 0)
            xPos -= 1;
        #endregion

        pos.x = xPos;
        pos.y = yPos;

        return pos;
    }

    void PlaceTile(Tile tileToPlace, Vector2 gridPos, int layerID, int undoPlayerID = -1)
    {
        tileToPlace.gridPos = gridPos;
        tileToPlace.layerID = layerID;
        if (LayerManager.instance.GetLayer(layerID).allTiles.Count > 0)
            tileToPlace.id = LayerManager.instance.GetLayer(layerID).allTiles[LayerManager.instance.GetLayer(layerID).allTiles.Count - 1].id + 1;
        else
            tileToPlace.id = 0;
        if (!IsCellEmpty(layerID, gridPos))
        {
            DestroyCell(layerID, gridPos, undoPlayerID);
        }
        if (tileToPlace.tileColor != Color.clear)
        {
            SpawnLoadedTile(tileToPlace, layerID);

            LayerManager.instance.GetLayer(layerID).allTiles.Add(tileToPlace);
            if (undoPlayerID == -2)
                UndoActions.instance.AddDataToCurrentAction(ActionData.ActionType.PlaceTile, tileToPlace);

        }
        else
        {
            DestroyCell(layerID, gridPos, undoPlayerID);
        }

    }

    public void SpawnLoadedTile(Tile tileToPlace, int layerID)
    {
        UnityEngine.Tilemaps.Tile newTile = tilemapTiles[(int)tileToPlace.type];
        newTile.color = tileToPlace.tileColor;

        LayerManager.instance.GetLayer(layerID).tilemap.SetTile(tileToPlace.gridPos, null);
        LayerManager.instance.GetLayer(layerID).tilemap.SetTile(tileToPlace.gridPos, newTile);
    }

    bool IsCellEmpty(int layerID, Vector2 cellToCheck)
    {
        for (int i = 0; i < LayerManager.instance.layers[layerID].allTiles.Count; i++)
        {
            if (LayerManager.instance.GetLayer(layerID).allTiles[i].gridPos == cellToCheck)
                return false;
        }

        return true;
    }

    public void DestroyCell(int layerID, Vector2 cellToDestroy, int undoPlayerID)
    {
        for (int i = 0; i < LayerManager.instance.layers[layerID].allTiles.Count; i++)
        {
            if (LayerManager.instance.GetLayer(layerID).allTiles[i].gridPos == cellToDestroy && LayerManager.instance.GetLayer(layerID).allTiles[i].destroyed == false)
            {
                //print("Destroyed at " + cellToDestroy + "(undoPlayerID: " + undoPlayerID + ")");
                LayerManager.instance.GetLayer(layerID).allTiles[i].destroyed = true;
                LayerManager.instance.GetLayer(layerID).tilemap.SetTile(LayerManager.instance.GetLayer(layerID).allTiles[i].gridPos, null);

                if (undoPlayerID == -2)
                {
                    UndoActions.instance.AddDataToCurrentAction(ActionData.ActionType.RemoveTile, LayerManager.instance.GetLayer(layerID).allTiles[i]);
                }

                LayerManager.instance.GetLayer(layerID).allTiles.RemoveAt(i);
            }
        }
    }

    public Tile GetCellByGridPos(int layerID, Vector2 cellGridPos)
    {
        for (int i = 0; i < LayerManager.instance.layers[layerID].allTiles.Count; i++)
        {
            if (LayerManager.instance.GetLayer(layerID).allTiles[i].gridPos == cellGridPos)
                return LayerManager.instance.GetLayer(layerID).allTiles[i];
        }

        return null;
    }
}

[System.Serializable]
public class Tile
{
    public enum TileType
    {
        Square,
        Circle,
        Plus
    }
    public TileType type;
    public SerializableVector2 gridPos;
    public SerializableColor tileColor;
    public int id;
    public int layerID;

    public bool destroyed = false;

    public float rotationDegrees;
}

/// <summary>
/// Since unity doesn't flag the Vector2 as serializable, we
/// need to create our own version. This one will automatically convert
/// between Vector2 and SerializableVector2
/// </summary>
[System.Serializable]
public struct SerializableVector2
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    public SerializableVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return System.String.Format("[{0}, {1}]", x, y);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector2 to Vector2
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector2(SerializableVector2 rValue)
    {
        return new Vector2(rValue.x, rValue.y);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector2 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector2 rValue)
    {
        return new Vector3(rValue.x, rValue.y, -10f);
    }

    /// <summary>
    /// Automatic conversion from Vector2 to SerializableVector2
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector2(Vector2 rValue)
    {
        return new SerializableVector2(rValue.x, rValue.y);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector2
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector2(Vector3 rValue)
    {
        return new SerializableVector2(rValue.x, rValue.y);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector2 to Vector3Int
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3Int(SerializableVector2 rValue)
    {
        return new Vector3Int((int)rValue.x, (int)rValue.y, -10);
    }

    public static bool operator ==(SerializableVector2 a, SerializableVector2 b)
    {
        if (a.x == b.x && a.y == b.y)
            return true;

        return false;
    }

    public static bool operator !=(SerializableVector2 a, SerializableVector2 b)
    {
        if (a.x != b.x || a.y != b.y)
            return true;

        return false;
    }

    public static bool operator ==(SerializableVector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y)
            return true;

        return false;
    }

    public static bool operator !=(SerializableVector2 a, Vector2 b)
    {
        if (a.x != b.x || a.y != b.y)
            return true;

        return false;
    }
}

/// <summary>
/// Since unity doesn't flag the Color as serializable, we
/// need to create our own version. This one will automatically convert
/// between Vector2 and SerializableVector2
/// </summary>
[System.Serializable]
public struct SerializableColor
{
    /// <summary>
    /// red
    /// </summary>
    public float r;

    /// <summary>
    /// green
    /// </summary>
    public float g;

    /// <summary>
    /// glue
    /// </summary>
    public float b;

    /// <summary>
    /// alpha
    /// </summary>
    public float a;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_r"></param>
    /// <param name="_g"></param>
    /// <param name="_b"></param>
    /// <param name="_a"></param>
    public SerializableColor(float _r, float _g, float _b, float _a)
    {
        r = _r;
        g = _g;
        b = _b;
        a = _a;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return System.String.Format("[{0}, {1}, {2}, {3}]", r, g, b, a);
    }

    /// <summary>
    /// Automatic conversion from SerializableColor to Color
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Color(SerializableColor rValue)
    {
        return new Color(rValue.r, rValue.g, rValue.b, rValue.a);
    }

    /// <summary>
    /// Automatic conversion from Color to SerializableColor
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableColor(Color rValue)
    {
        return new SerializableColor(rValue.r, rValue.g, rValue.b, rValue.a);
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapePicker : MonoBehaviour {
    bool menuOpen;
    [SerializeField]
    Image panel;
    [SerializeField]
    Image closePanelButton;
    [SerializeField]
    Image SelectedShapePreview;

    GridManager gridManager;
    int shapeID;

    public static ShapePicker instance;

    private void Start()
    {
        gridManager = GridManager.instance;
        instance = this;
    }

    public void SelectShape(int _id)
    {
        shapeID = _id;
        ApplyShape();
    }
    public void ApplyShape()
    {
        gridManager.shapeID = shapeID;
        //SelectedShapePreview.sprite = gridManager.tiles[shapeID].GetComponent<SpriteRenderer>().sprite;
        SelectedShapePreview.sprite = gridManager.tilemapTiles[shapeID].sprite;

        closePanelButton.rectTransform.localRotation = new Quaternion(0, 0, 180, 0);
        panel.gameObject.SetActive(false);
        menuOpen = false;
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
}

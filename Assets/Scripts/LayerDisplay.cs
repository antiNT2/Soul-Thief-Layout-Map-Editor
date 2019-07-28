using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LayerDisplay : MonoBehaviour
{
    LayerManager layerManager;
    public int thisLayer;

    [SerializeField]
    Text layerName;
    [SerializeField]
    Button toggleVisibility;
    [SerializeField]
    InputField editNameField;

    Image box;

    string oldLayerName; //used by the rename layer function

    void Start()
    {
        layerManager = LayerManager.instance;
        box = GetComponent<Image>();
        layerManager.GetLayer(thisLayer).layerDisplay = this.gameObject;
    }

    void Update()
    {
        if (layerManager.GetLayer(thisLayer).deleted == true)
            this.gameObject.SetActive(false);
        layerName.text = layerManager.GetLayer(thisLayer).layerName;

        if (layerManager.GetLayer(thisLayer).visible)
        {
            toggleVisibility.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            toggleVisibility.GetComponent<Image>().color = new Color(0.5f, 1f, 1f, 0.2f);
        }
        if (layerManager.GetLayer(thisLayer).beginEditName == true && editNameField.gameObject.activeSelf == false)
            EnableEditNameField();

        #region Arrows
        /* if (layerManager.GetLayer(thisLayer).sortingOrder == 0)
             upArrow.gameObject.SetActive(false);
         else
             upArrow.gameObject.SetActive(true);
         if (thisLayer + 1 == layerManager.layers.Count)
             downArrow.gameObject.SetActive(false);
         else
             downArrow.gameObject.SetActive(true);*/
        #endregion

        if (layerManager.activeLayer == thisLayer)
            box.color = new Color32(25, 39, 53, 255);
        else
            box.color = new Color32(44, 62, 80, 255);
    }

    public void SetActiveLayer()
    {
        layerManager.activeLayer = thisLayer;
    }

    void EnableEditNameField()
    {
        editNameField.gameObject.SetActive(true);
        editNameField.Select();
        oldLayerName = layerManager.GetLayer(thisLayer).layerName;
        layerManager.GetLayer(thisLayer).layerName = "";
        editNameField.text = layerManager.GetLayer(thisLayer).layerName;
    }

    public void FinishEditingName()
    {
        LayerManager.instance.GetLayer(thisLayer).beginEditName = false;
        editNameField.gameObject.SetActive(false);
        if (editNameField.text == "")
            editNameField.text = oldLayerName;
        Manager.localPlayerManager.CmdRenameLayer(thisLayer, editNameField.text);
    }

    public void ToggleVisibility()
    {
        layerManager.SetLayerVisibility(thisLayer, !layerManager.GetLayer(thisLayer).visible);        
    }
}

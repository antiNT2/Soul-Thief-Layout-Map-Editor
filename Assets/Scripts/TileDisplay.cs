using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDisplay : MonoBehaviour
{
    SpriteRenderer tileSprite;
    public int tileID;
    public int layerID;

    LayerManager layerManager;

    void Start()
    {
        tileSprite = GetComponent<SpriteRenderer>();
        this.name = tileID.ToString();
        layerManager = LayerManager.instance;
    }

    private void Update()
    {
        if (layerManager.GetLayer(layerID) == null || layerManager.GetLayer(layerID).GetTile(tileID) == null)
            Destroy(this.gameObject);

        if (layerManager.GetLayer(layerID).GetTile(tileID).destroyed)
        {
            layerManager.GetLayer(layerID).allTiles.Remove(layerManager.GetLayer(layerID).GetTile(tileID));
            Destroy(this.gameObject);
        }

        if (layerManager.GetLayer(layerID).visible == false)
            tileSprite.enabled = false;
        else if (layerManager.GetLayer(layerID).visible == true)
            tileSprite.enabled = true;

        if (layerManager.GetLayer(layerID).deleted == true)
        {
            layerManager.GetLayer(layerID).allTiles.Remove(layerManager.GetLayer(layerID).GetTile(tileID));
            Destroy(this.gameObject);
        }
    }

    public void SetColor(Color colorToSet)
    {
        if(tileSprite == null)
            tileSprite = GetComponent<SpriteRenderer>();

        tileSprite.color = colorToSet;
    }

}

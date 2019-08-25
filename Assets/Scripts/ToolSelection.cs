using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelection : MonoBehaviour
{
    public enum ToolType
    {
        Paint,
        SelectionRectangle
    }
    public ToolType currentTool;
    public static ToolSelection instance;
    public Image[] buttons;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SelectTool(0);
    }

    public void SelectTool(int id)
    {
        currentTool = (ToolType)id;
        SetColor();
    }

    void SetColor()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].color = new Color(0.4980392f, 0.5490196f, 0.5529412f);
        }
        buttons[(int)currentTool].color = Color.white;
    }
}

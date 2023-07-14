using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using XDPaint.Controllers;
using XDPaint.Controllers.InputData;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject;
using XDPaint.Core.PaintObject.Base;
using XDPaint.States;
using XDPaint.Tools;
using XDPaint.Tools.Image.Base;
using XDPaint.Tools.Layers;
using XDPaint.Tools.Raycast;
using XDPaint.Tools.Triangles;
using XDPaint.Utils;
using XDPaint.Demo.UI;




public class PatintForK : MonoBehaviour
{
    public PaintController PaintBorad;
    public ToolToggle[] ToolS;
    public Texture[] BrushTextureS;

    public ImageSwitcher _imageSwitcher;

    
    private void Update()

    {
        if (Input.GetKey(KeyCode.Mouse1))
        {

            ClearTool();
            _imageSwitcher.SwitchImage(0);
        }
        
    }

    public void ClearTool()//没有工具的时候就是一根画不出来颜色的笔
    {
        PaintBorad.Brush.SetTexture(BrushTextureS[0], true, false);
        PaintBorad.Tool = ToolS[0].Tool;
        PaintBorad.Brush.SetColor(new Color(0, 0, 0, 0));
        Debug.Log("Clear");
    }

    public void ChoseEraserhOne()
    {
        PaintBorad.Brush.SetTexture(BrushTextureS[2], true, false);
        PaintBorad.Tool = ToolS[1].Tool;
        PaintBorad.Brush.SetColor(new Color(1, 1, 1, 1f));

    }

    public void ChoseBrushOne()
    {
        PaintBorad.Brush.SetTexture(BrushTextureS[1], true, false);
        PaintBorad.Tool = ToolS[0].Tool;
        PaintBorad.Brush.SetColor(new Color(0.4f,0.3f,0.5f,1f));

    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于控制每个星星预制体的属性和运动
/// Star 较其他 PageControlAble 变化较大
/// </summary>
public class Star : PageControlAble
{
    /// <summary>
    /// 视差灵敏度
    /// </summary>
    public float Parallax;

    public override void Update()
    {
        float xPos = (PosRatio.x - (CurrentPage - 1) * Parallax) * PanelSize.x;
        float yPos = PosRatio.y * PanelSize.y;
        RectTransform.localPosition = new Vector3(xPos, yPos, PosRatio.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 可被 PageController 常规控制的组件
/// </summary>
public abstract class PageControlAble : MonoBehaviour
{
    #region 变量
    /// <summary>
    /// 当前页面进度
    /// </summary>
    public float CurrentPage;

    /// <summary>
    /// 这个组件在第几页可用？
    /// </summary>
    public int AblePage;
    /// <summary>
    /// 这个组件目前是否可用？
    /// </summary>
    public bool IsAble;

    /// <summary>
    /// 当前游戏分辨率宽高
    /// </summary>
    public Vector2 PanelSize;

    /// <summary>
    /// 该组件在可用页时的位置比例（以左下为原点，相对于PanelWidth）
    /// </summary>
    public Vector3 PosRatio;

    /// <summary>
    /// 该组件在可用页时的透明度
    /// </summary>
    public float Alpha;

    /// <summary>
    /// 该组件在可用页的大小
    /// </summary>
    public Vector3 Size;

    /// <summary>
    /// 自身的 RectTransform 组件
    /// </summary>
    public RectTransform RectTransform;
    #endregion

    public void Start()
    {
        CurrentPage = 1f;
        RectTransform = GetComponent<RectTransform>();
        RectTransform.localScale = Size;
        Update();
    }

    public virtual void Update() { }

}

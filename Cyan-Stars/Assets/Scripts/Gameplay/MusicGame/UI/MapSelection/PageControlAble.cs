using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 可被 PageController 常规控制的组件
/// </summary>
public class PageControlAble : MonoBehaviour
{
    #region 变量
    /// <summary>
    /// 这个组件在第几页可用？
    /// </summary>
    public int AblePage;

    /// <summary>
    /// 用 PosRatio 覆盖 RectTransform 位置
    /// 若 False，则自动获取当前位置
    /// </summary>
    public bool OverridePos = false;

    /// <summary>
    /// 该组件在可用页时的位置比例（以左下为原点，相对于PanelWidth）
    /// </summary>
    public Vector3 PosRatio;

    /// <summary>
    /// 位移灵敏度
    /// </summary>
    public Vector3 PosParallax;

    /// <summary>
    /// 该组件在可用页时的透明度
    /// </summary>
    public float Alpha = 1f;

    /// <summary>
    /// 透明度灵敏度
    /// </summary>
    public float AlphaParallax;

    /// <summary>
    /// 该组件在可用页的大小
    /// </summary>
    public Vector3 Size = new Vector3(1f, 1f, 1f);

    /// <summary>
    /// 大小灵敏度
    /// </summary>
    public Vector3 SizeParallax;

    // ---------------------------------

    /// <summary>
    /// 当前页面进度
    /// </summary>
    [HideInInspector]
    public float CurrentPage;

    /// <summary>
    /// 这个组件目前是否可用？
    /// </summary>
    [HideInInspector]
    public bool IsAble;

    /// <summary>
    /// 当前游戏分辨率宽高
    /// </summary>
    [HideInInspector]
    public Vector2 PanelSize;

    /// <summary>
    /// 自身的 RectTransform 组件
    /// </summary>
    [HideInInspector]
    public RectTransform RectTransform;

    /// <summary>
    /// 自身及子节点的 Image 组件
    /// </summary>
    [HideInInspector]
    public Image[] Images;

    /// <summary>
    /// 自身及子节点的 Button 组件
    /// </summary>
    [HideInInspector]
    public Button[] Buttons;

    public TMP_Text[] TextMeshes;
    #endregion

    public virtual void Start()
    {
        CurrentPage = 1f;
        RectTransform = GetComponent<RectTransform>();
        RectTransform.localScale = Size;
        Images = GetComponentsInChildren<Image>();
        Buttons = GetComponentsInChildren<Button>();
        TextMeshes = GetComponentsInChildren<TMP_Text>();
        if (!OverridePos)
        {
            float x = RectTransform.localPosition.x / PanelSize.x;
            float y = RectTransform.localPosition.y / PanelSize.y;
            float z = RectTransform.localPosition.z;
            PosRatio = new Vector3(x, y, z);
        }
    }

    public virtual void Update()
    {
        float deltaPage = Mathf.Abs(CurrentPage - AblePage);
        ChangePos(deltaPage);
        ChangeAlpha(deltaPage);
        ChangeSize(deltaPage);
    }

    public virtual void ChangePos(float dp)
    {
        float x = (PosRatio.x + PosParallax.x * dp) * PanelSize.x;
        float y = (PosRatio.y + PosParallax.y * dp) * PanelSize.y;
        float z = PosRatio.z + PosParallax.z * dp;
        RectTransform.localPosition = new Vector3(x, y, z);
    }

    public virtual void ChangeAlpha(float dp)
    {
        gameObject.SetActive(true);
        if (Images.Length != 0)
        {
            foreach (Image image in Images)
            {
                Color color = image.color;
                color.a = Alpha - AlphaParallax * dp;
                image.color = color;
            }
        }

        if (Buttons.Length != 0)
        {
            foreach (Button button in Buttons)
            {
                if (Alpha - AlphaParallax * dp <= 0.01f)
                {
                    button.interactable = false;
                }
                else
                {
                    button.interactable = true;
                }
            }
        }

        if (TextMeshes.Length != 0)
        {
            foreach (TMP_Text textMesh in TextMeshes)
            {
                textMesh.alpha = Alpha - AlphaParallax * dp;
            }
        }
    }

   

    public virtual void ChangeSize(float dp)
    {
        float x = Size.x + SizeParallax.x * dp;
        float y = Size.y + SizeParallax.y * dp;
        float z = Size.z + SizeParallax.z * dp;
        RectTransform.localScale = new Vector3(x, y, z);
    }
}

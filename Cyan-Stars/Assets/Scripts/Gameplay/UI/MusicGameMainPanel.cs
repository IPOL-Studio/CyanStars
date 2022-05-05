using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.UI
{
    /// <summary>
    /// 音游主界面
    /// </summary>
    [UIData(UIGroupName = Consts.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGameMainPanel.prefab")]
    public class MusicGameMainPanel : BaseUIPanel
    {
        public Image ImgProgress;
        public TextMeshProUGUI TxtCombo;
        public TextMeshProUGUI TxtScore;
        public Transform TransFrame;
        public Button BtnStart;
        public TextMeshProUGUI TxtLrc;
    }
}


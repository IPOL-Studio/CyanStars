using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.UI
{
    /// <summary>
    /// 音游3DUI界面
    /// </summary>
    [UIData(UIGroupName = Consts.UIGroup3D,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGame3DUIPanel.prefab")]
    public class MusicGame3DUIPanel : BaseUIPanel
    {
        public TextMeshProUGUI TxtGrad;
        public TextMeshProUGUI TxtAccuracy;
        public TextMeshProUGUI TxtScoreRatio;
        public TextMeshProUGUI TxtVisibleScore;
    }
}
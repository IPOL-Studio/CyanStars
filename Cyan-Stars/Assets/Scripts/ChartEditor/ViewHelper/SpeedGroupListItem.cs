using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.ViewHelper
{
    [RequireComponent(typeof(Toggle))]
    public class SpeedGroupListItem : MonoBehaviour
    {
        public Toggle Toggle;
        public TMP_Text IndexText;
        public TMP_Text RemarkText;
    }
}

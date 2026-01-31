#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    public class PopupButton : MonoBehaviour
    {
        [SerializeField]
        public Button Button = null!;

        [SerializeField]
        public TMP_Text Text = null!;
    }
}

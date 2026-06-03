#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    [RequireComponent(typeof(Button))]
    public class PlayPauseButton : MonoBehaviour
    {
        public Button Button = null!;
        public Image Image = null!;
    }
}

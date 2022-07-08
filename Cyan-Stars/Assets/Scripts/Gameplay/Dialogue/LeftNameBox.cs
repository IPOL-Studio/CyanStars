using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class LeftNameBox : Image
    {
        private TMP_Text text;

        protected override void Start()
        {
            text = GetComponentInChildren<TMP_Text>();
        }
    }
}


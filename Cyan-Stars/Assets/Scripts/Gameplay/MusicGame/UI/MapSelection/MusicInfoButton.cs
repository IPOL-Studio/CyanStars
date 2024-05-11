using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SyanStars.Gameplay.MapSelection
{
    public class MusicInfoButton : MonoBehaviour
    {
        public string URL;

        public void OpenURL()
        {
            Application.OpenURL(URL);
        }
    }
}


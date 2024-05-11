using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
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


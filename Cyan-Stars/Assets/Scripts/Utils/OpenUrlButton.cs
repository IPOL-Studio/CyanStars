using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Utils
{
    [RequireComponent(typeof(Button))]
    public class OpenUrlButton : MonoBehaviour
    {
        public string URL;

        public void OpenURL()
        {
            Application.OpenURL(URL);
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OpenURL);
        }
    }
}


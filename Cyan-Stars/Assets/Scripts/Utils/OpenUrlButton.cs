using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Utils
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


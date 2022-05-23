using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CyanStars.Framework.UI
{
    [RequireComponent(typeof(Camera))]
    public class AddUICameraToStack : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Camera>().GetUniversalAdditionalCameraData().cameraStack.Add(GameRoot.UI.UICamera);
        }
    }
}

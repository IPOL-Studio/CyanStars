using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 资源绑定器，用于自动卸载资源
    /// </summary>
    public class AssetBinder : MonoBehaviour
    {
        [SerializeField]
        private List<Object> bindingAssets = new List<Object>();

        /// <summary>
        /// 绑定Asset
        /// </summary>
        public void BindTo(Object asset)
        {
            bindingAssets.Add(asset);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < bindingAssets.Count; i++)
            {
                GameRoot.Asset.UnloadAsset(bindingAssets[i]);
            }
        }
    }
}

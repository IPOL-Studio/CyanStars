using System;
using System.Collections;
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
        public List<Object> BindingAssets = new List<Object>();

        /// <summary>
        /// 绑定Asset
        /// </summary>
        public void BindTo(Object asset)
        {
            BindingAssets.Add(asset);
        }
        
        private void OnDestroy()
        {
            for (int i = 0; i < BindingAssets.Count; i++)
            {
                GameRoot.Asset.UnloadAsset(BindingAssets[i]);
            }
        }
    }
}


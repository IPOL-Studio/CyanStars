using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework;


namespace CyanStars.Gameplay.MusicGame
{
    public class KeyViewController : MonoBehaviour //view层的Key控制器
    {
        [Header("Key预制体"), SerializeField]
        private GameObject keyPrefab; //key预制体

        private readonly Dictionary<KeyCode, GameObject> KeyDict = new Dictionary<KeyCode, GameObject>(); //key列表

        public void KeyDown(InputMapData.Item item)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                return;
            }

            if (item.Key == KeyCode.Space) return;
            if (KeyDict.TryGetValue(item.Key, out var key))
            {
                key.SetActive(true);
            }
            else
            {
                key = Instantiate(keyPrefab);
                var trans = key.transform;
                trans.position = new Vector3(Endpoint.Instance.GetPosWithRatio(item.RangeMin), 0, 20);
                trans.localScale = new Vector3(Endpoint.Instance.Length * item.RangeWidth, 0.1f, 10000);
                trans.SetParent(transform);
                key.name = item.Key.ToString();
                KeyDict.Add(item.Key, key);
            }
        }

        public void KeyUp(InputMapData.Item item)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                return;
            }

            if (KeyDict.TryGetValue(item.Key, out var key))
            {
                key.SetActive(false);
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using CyanStars.Gameplay.Note;

namespace CyanStars.Gameplay.Input
{
    public class KeyViewController : MonoBehaviour //view层的Key控制器
    {
        [Header("Key预制体")] public GameObject keyPrefab; //key预制体
        public Dictionary<KeyCode, GameObject> keyList = new Dictionary<KeyCode, GameObject>(); //key列表

        public void KeyDown(InputMapData.Item item)
        {
            if (GameManager.Instance.isAutoMode)
            {
                return;
            }

            if (item.key == KeyCode.Space) return;
            if (keyList.TryGetValue(item.key, out var key))
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
                key.name = item.key.ToString();
                keyList.Add(item.key, key);
            }
        }

        public void KeyUp(InputMapData.Item item)
        {
            if (GameManager.Instance.isAutoMode)
            {
                return;
            }

            if (keyList.TryGetValue(item.key, out var key))
            {
                key.SetActive(false);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework;


namespace CyanStars.Gameplay.MusicGame
{
    public class KeyViewController : MonoBehaviour //view层的Key控制器
    {
        [Header("Key预制体"), SerializeField]
        private GameObject keyPrefab; //key预制体

        // ID -> GameObject
        private readonly Dictionary<int, GameObject> KeyDict = new Dictionary<int, GameObject>();

        public void KeyDown(InputEventArgs e)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                return;
            }

            if (e.RangeMin < 0 || e.RangeMin > 1) return;

            if (KeyDict.TryGetValue(e.ID, out var key))
            {
                key.SetActive(true);
            }
            else
            {
                key = Instantiate(keyPrefab);
                var trans = key.transform;
                trans.position = new Vector3(Endpoint.Instance.GetPosWithRatio(e.RangeMin), 0, 20);
                trans.localScale = new Vector3(Endpoint.Instance.Length * e.RangeWidth, 0.1f, 10000);
                trans.SetParent(transform);
                key.name = e.ID.ToString();
                KeyDict.Add(e.ID, key);
            }
        }

        public void KeyUp(InputEventArgs e)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                return;
            }

            if (KeyDict.TryGetValue(e.ID, out var key))
            {
                key.SetActive(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.MusicGame
{
    public class TouchInputReceiverGenerator: MonoBehaviour
    {

        private const float Width = 30f;

        private InputMapData inputMapData;

        public GameObject Prefab;
        public Transform Parent;

        public void SetInputMapData(InputMapData inputMapData)
        {
            this.inputMapData = inputMapData;

            for (int i = 0; i < 12; i++)
            {
                GameObject obj = Instantiate(Prefab,default,default,Parent);

                InputMapData.Item item = inputMapData.Items[i];
                Vector3 pos = default;
                pos.x = item.RangeMin * Width;
                obj.transform.localPosition = pos;

                obj.GetComponent<TouchInputReceiver>().SetKeyItem(item);
            }
        }

    }
}


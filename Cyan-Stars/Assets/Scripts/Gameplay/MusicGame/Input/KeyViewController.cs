using System;
using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework;


namespace CyanStars.Gameplay.MusicGame
{
    public class KeyViewController : MonoBehaviour //view层的Key控制器
    {
        [Header("Key预制体"), SerializeField]
        private GameObject keyPrefab; //key预制体

        private readonly Dictionary<int, GameObject> KeyDict = new Dictionary<int, GameObject>(); //key列表

        private void Awake()
        {
            GameRoot.Event.AddListener(InputEventArgs.EventName,OnInput);
            GameRoot.Event.AddListener(EventConst.MusicGameEndEvent,OnMusicGameEnd);
        }

        private void OnDestroy()
        {
            GameRoot.Event.RemoveListener(InputEventArgs.EventName,OnInput);
            GameRoot.Event.RemoveListener(EventConst.MusicGameEndEvent,OnMusicGameEnd);
        }

        private void OnMusicGameEnd(object sender, EventArgs e)
        {
            foreach (KeyValuePair<int,GameObject> pair in KeyDict)
            {
                pair.Value.SetActive(false);
            }
        }


        private void OnInput(object sender, EventArgs e)
        {
            InputEventArgs args = (InputEventArgs)e;

            switch (args.Type)
            {
                case InputType.Down:
                    InputDown(args);
                    break;

                case InputType.Up:
                    InputUp(args);
                    break;
            }
        }


        private void InputDown(InputEventArgs args)
        {

            if (args.RangeWidth == 0) return;

            if (KeyDict.TryGetValue(args.ID, out var key))
            {
                key.SetActive(true);
            }
            else
            {
                key = Instantiate(keyPrefab);
                var trans = key.transform;
                trans.position = new Vector3(Endpoint.Instance.GetPosWithRatio(args.RangeMin), 0, 20);
                trans.localScale = new Vector3(Endpoint.Instance.Length * args.RangeWidth, 0.1f, 10000);
                trans.SetParent(transform);
                KeyDict.Add(args.ID, key);
            }
        }

        private void InputUp(InputEventArgs args)
        {
            if (KeyDict.TryGetValue(args.ID, out var key))
            {
                key.SetActive(false);
            }
        }
    }
}

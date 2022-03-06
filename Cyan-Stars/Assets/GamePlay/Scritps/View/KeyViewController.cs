using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyViewController : MonoBehaviour
{
    [Header("Key预制体")]
    public GameObject keyPrefab;
    public Dictionary<KeyCode,GameObject> keyList = new Dictionary<KeyCode, GameObject>();
    public void KeyDown(InputMapData.Item item)
    {
        if (keyList.TryGetValue(item.key, out var key))
        {
            key.SetActive(true);
        }
        else
        {
            key = Instantiate(keyPrefab);
            var trans = key.transform;
            trans.position = new Vector3(item.RangeMin * 9.8f - 13.66f,0,0);
            trans.localScale = new Vector3((item.RangeMax - item.RangeMin)*10, 0.1f, 115);
            trans.SetParent(transform);
            key.name = item.key.ToString();
            keyList.Add(item.key, key); 
        }
    }
    public void KeyUp(InputMapData.Item item)
    {
        if (keyList.TryGetValue(item.key, out var key))
        {
            key.SetActive(false);
        }
    }
}

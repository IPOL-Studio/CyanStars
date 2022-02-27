using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyViewController : MonoBehaviour
{
    [Header("Key预制体")]
    public GameObject keyPrefab;
    private Dictionary<KeyCode,GameObject> keyList = new Dictionary<KeyCode, GameObject>();
    public void keyDown(InputMapData.Item item)
    {
        Transform tf = new GameObject().transform;
        tf.position = new Vector3(item.RangeMin * 10 - 12.3f,0,0);
        tf.localScale = new Vector3((item.RangeMax - item.RangeMin)*10, 1, 1);
        GameObject key = Instantiate(keyPrefab, tf);
        key.transform.SetParent(transform);
        keyList[item.key] = key;
    }
    public void keyUp(InputMapData.Item keyCode)
    {
        if (keyList.ContainsKey(keyCode.key))
        {
            Destroy(keyList[keyCode.key]);
            keyList.Remove(keyCode.key);
        }
    }
}

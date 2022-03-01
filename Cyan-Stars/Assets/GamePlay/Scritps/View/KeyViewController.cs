using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyViewController : MonoBehaviour
{
    [Header("Key预制体")]
    public GameObject keyPrefab;
    public Dictionary<KeyCode,GameObject> keyList = new Dictionary<KeyCode, GameObject>();
    public void keyDown(InputMapData.Item item)
    {
        /*
        Transform tf = new GameObject().transform;
        tf.position = new Vector3(item.RangeMin * 10.2f - 13.66f,0,0);
        tf.localScale = new Vector3((item.RangeMax - item.RangeMin)*10, 1, 1);
        */
        if(!keyList.ContainsKey(item.key))
        {
            GameObject key = Instantiate(keyPrefab);
            key.transform.position = new Vector3(item.RangeMin * 10.2f - 13.66f,0,0);
            key.transform.localScale = new Vector3((item.RangeMax - item.RangeMin)*10, 0.1f, 115);
            key.transform.SetParent(transform);
            key.name = item.key.ToString();
            keyList[item.key] = key;
        }
        else
        {
            keyList[item.key].SetActive(true);
        }
    }
    public void keyUp(InputMapData.Item item)
    {
        if (keyList.ContainsKey(item.key))
        {
            keyList[item.key].SetActive(false);
            //keyList.Remove(keyCode.key);
        }
    }
}

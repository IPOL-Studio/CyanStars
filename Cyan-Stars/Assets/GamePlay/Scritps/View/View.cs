using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视图层物体挂载的脚本
/// </summary>
public class View : MonoBehaviour, IView
{
    private float deltaTime;
    private Collider other;
    public bool isTiggered;
    public GameObject effectPrefab;
    
    public void OnUpdate(float deltaTime)
    {
        this.deltaTime = deltaTime;
        
        Vector3 pos = transform.position;
        pos.z -= deltaTime * 2;
        transform.position = pos;
    }
    public bool IsTiggered()
    {
        return isTiggered;
    }
    public Transform GetTransform()
    {
        return transform;
    }
    void Update()
    {
        if(isTiggered && (!other || !other.gameObject.activeSelf))
        {
            isTiggered = false;
            this.other = null;
        }
    }

    /// <summary>
    /// 销毁自身（可以不马上销毁，而是再自动移动一段后再销毁，通常用于漏掉音符的情况）
    /// </summary>
    public void DestroySelf(bool autoMove = true,float destroyTime = 2f)
    {
        if (!autoMove)
        {
            GameObject.Instantiate(effectPrefab,transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }
        
        StartCoroutine(AutoMove(destroyTime));
    }

    /// <summary>
    /// 自动移动一段时间然后销毁自己
    /// </summary>
    public IEnumerator AutoMove(float destroyTime)
    {
        float timer = 0;
        while (true)
        {
            timer += deltaTime;
            
            Vector3 pos = transform.position;
            pos.z -= deltaTime * 2;
            transform.position = pos;

            if (timer >= destroyTime)
            {
                Destroy(gameObject);
                yield break;
            }
            
            yield return null;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Key")
        {
            isTiggered = true;
            this.other = other;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视图层物体挂载的脚本
/// </summary>
public class View : MonoBehaviour, IView
{
    private float deltaTime;
    
    public void OnUpdate(float deltaTime)
    {
        this.deltaTime = deltaTime;
        
        Vector3 pos = transform.position;
        pos.x -= deltaTime;
        transform.position = pos;
    }

    /// <summary>
    /// 销毁自身（可以不马上销毁，而是再自动移动一段后再销毁，通常用于漏掉音符的情况）
    /// </summary>
    public void DestorySelf(bool autoMove = true)
    {
        if (!autoMove)
        {
            Destroy(gameObject);
            return;
        }
        
        StartCoroutine(AutoMove());
    }

    /// <summary>
    /// 自动移动一段时间然后销毁自己
    /// </summary>
    public IEnumerator AutoMove()
    {
        float timer = 0;
        while (true)
        {
            timer += deltaTime;
            
            Vector3 pos = transform.position;
            pos.x -= deltaTime;
            transform.position = pos;

            if (timer >= 2f)
            {
                Destroy(gameObject);
                yield break;
            }
            
            yield return null;
        }
    }


}

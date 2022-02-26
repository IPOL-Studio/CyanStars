using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 视图层物体脚本
/// </summary>
public class ViewObject : MonoBehaviour, IView
{
    private float deltaTime;
    
    public void OnUpdate(float deltaTime)
    {
        this.deltaTime = deltaTime;
        
        Vector3 pos = transform.position;
        pos.y -= deltaTime;
        transform.position = pos;
    }

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
            pos.y -= deltaTime;
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
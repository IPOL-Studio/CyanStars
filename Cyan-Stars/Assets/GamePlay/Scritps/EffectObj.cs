using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EffectObj : MonoBehaviour
{
    public float destroyTime = -1;
    public VisualEffect visualEffect;
    public float visualEffectStartCount = -1;

    void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
        if(visualEffectStartCount <= 0)visualEffectStartCount = visualEffect.GetFloat("Count");
        if(destroyTime > 0)
        {
            StartCoroutine(DestroySelf());
        }
    }
    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds((destroyTime - 3000f)/1000);
        for(int i = 200;i > 0;i --)
        {
            visualEffect.SetFloat("Count",(i/2f)/100f * visualEffectStartCount);
            yield return null;
        }
        Destroy(gameObject,1);
    }
}

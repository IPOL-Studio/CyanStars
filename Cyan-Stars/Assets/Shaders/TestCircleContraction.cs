using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TestCircleContraction : MonoBehaviour
{
    public Material material;
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private bool a = true;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (a)
            {
                case true:
                    material.DOFloat(1, Slider, 1).SetEase(Ease.InOutExpo);
                    a = !a;
                    break;
                case false:
                    material.DOFloat(0, Slider, 1).SetEase(Ease.InOutExpo);
                    a = !a;
                    break;
            }
        }
    }
}

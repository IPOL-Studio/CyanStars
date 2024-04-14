using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// StarsGenerator 用于控制选曲面板背景星星和 Staff 信息的生成、移动。
public class StarsGenerator : MonoBehaviour
{
    [Header("图像配置 Image settings")]
    [Tooltip("星星预制体")]
    public GameObject StarPrefab;
    [Tooltip("准星图像")]
    public GameObject SightPrefab;

    [Header("数值配置 Value settings")]
    [Tooltip("至少生成几个星星")]
    public int MinStarNum = 100;
    [Tooltip("至多生成几个星星")]
    public int MaxStarNum = 150;
    [Tooltip("星星的透明度至少为")]
    public float MinStarAlpha = 0.5f;
    [Tooltip("星星的透明度至多为")]
    public float MaxStarAlpha = 1.0f;
    [Tooltip("星星的大小至少为")]
    public float MinStarSize;
    [Tooltip("星星的大小至多为")]
    public float MaxStarSize;
    [Tooltip("由视差引起的位移至少为")]
    public float MinStarParallax;
    [Tooltip("由视差引起的位移至多为")]
    public float MaxStarParallax;

    
    [Header("Debug, readonly")]
    [SerializeField]
    [Tooltip("实际生成的星星数量")]
    private int starNum;

    void Start()
    {
        starNum = Random.Range(MinStarNum, MaxStarNum);
        for (int i = 0; i < starNum; i++)
        {

        }
    }

    void Update()
    {

    }
}

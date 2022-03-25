using UnityEngine;
using System.Collections;

public class ParticleSea : MonoBehaviour 
{
    [Header("粒子系统")]
    new public ParticleSystem particleSystem; // 粒子系统
    private ParticleSystem.Particle[] particlesArray; //存储粒子的数组

    [Header("粒子间空隙")]
    public float spacing = 0.25f; // 粒子之间的空隙

    [Header("粒子范围")]
    public int seaResolution; //粒子范围

    [Header("噪声参数")]
    public float noiseScale = 0.2f;  //噪声范围
    public float heightScale = 3f;  // 高度范围

    private float perlinNoiseAnimX = 0.01f; // 柏林噪声相关参数
    private float perlinNoiseAnimY = 0.01f;

    [Header("粒子颜色(随高度渐变)")]
    public Gradient colorGradient; // 颜色的渐变
    

    void Start() 
    {
        particlesArray = new ParticleSystem.Particle[seaResolution * seaResolution]; //初始化数组
        ParticleSystem.MainModule mainModule = particleSystem.main; //获取粒子系统的主要模块
        mainModule.maxParticles = seaResolution * seaResolution; //设置最大粒子数量
        particleSystem.Emit(seaResolution * seaResolution); //发射粒子，参数为要发射的粒子数量
        particleSystem.GetParticles(particlesArray);
    }

    void Update() 
    {
        for(int i = 0;i<seaResolution;i++) 
        {
            for(int j=0; j<seaResolution;j++)
            {
                float yPos = Mathf.PerlinNoise (i*noiseScale+perlinNoiseAnimX,j*noiseScale + perlinNoiseAnimY) * heightScale; // 由柏林噪声确定的高度值
                particlesArray [i * seaResolution + j].startColor = colorGradient.Evaluate (yPos); // 由高度值确定的颜色变化
                particlesArray [i * seaResolution + j].position = new Vector3 (i*spacing,yPos*heightScale,j*spacing);
            }
        }

        perlinNoiseAnimX += 0.01f;
        perlinNoiseAnimY += 0.01f;

        particleSystem.SetParticles(particlesArray, particlesArray.Length); // 设置该系统的粒子
    }
}

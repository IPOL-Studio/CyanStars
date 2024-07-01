using System.Collections;
using System.Collections.Generic;
using CyanStars.Graphics.Band;
using UnityEngine;

public class BandExample : MonoBehaviour
{
    private Band.BandData bandData;
    private Band band;
    private float[] bandHeights;
    void Start()
    {
        bandData.Count = 20;
        bandData.XSize = 100;
        bandData.YSize = 10;
        bandData.XOffset = 0.5f;
        bandData.YOffset = 0.2f;
        band = new Band(bandData);

        bandHeights = new float[bandData.Count + 1];
        for (int i = 0; i < bandHeights.Length; i++)
        {
            bandHeights[i] = Random.Range(0, 0.5f);
        }
        band.UpdateBand(bandHeights);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetronomeScript : MonoBehaviour
{
    public AudioClip clicp1, clicp2;
    AudioSource audioSource;
    public GameObject BPMInputFieldText, beatInputFieldText;
    float bpm = 0, sumTime = 0;
    int beat = 0, nowBeat = 0;
    bool awake = false;
    public Image image;
    float alpha;

    private void Start()
    { audioSource = this.GetComponent<AudioSource>(); }

    void Update()
    {
        alpha -= 510f * Time.deltaTime;
        if (alpha < 0) { alpha = 0; }
        if (awake)
        {
            sumTime += Time.deltaTime;
            if (sumTime >= 60 / bpm)
            {
                sumTime -= 60 / bpm;
                if (nowBeat == 0) { audioSource.clip = clicp1; }
                else { audioSource.clip = clicp2; }
                nowBeat++;
                if (nowBeat >= beat) { nowBeat -= beat; }
                alpha = 255f;
                audioSource.Play();
            }
        }
        else { sumTime = 0; nowBeat = 0; }
        image.GetComponent<Image>().color = new Color(102/255f, 204/255f, 255/255f, alpha/255f);
    }

    public void ReloadBPM()
    {
        try { bpm = float.Parse(BPMInputFieldText.GetComponent<Text>().text); }
        catch { bpm = 0; }
    }

    public void ReloadBeat()
    {
        try { beat = int.Parse(beatInputFieldText.GetComponent<Text>().text); }
        catch { beat = 0; }
    }

    public void OnButtonClick()
    {
        if (awake) { awake = false; }
        else { awake = true; sumTime += 60 / bpm; }
    }

    public void AddTime()
    { sumTime += 0.01f; }

    public void MinusTime()
    { sumTime -= 0.01f; }
}

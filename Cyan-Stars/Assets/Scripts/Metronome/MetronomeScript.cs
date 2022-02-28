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

    private void Start()
    { audioSource = this.GetComponent<AudioSource>(); }

    void Update()
    {
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
                audioSource.Play();
            }
        }
        else { sumTime = 0; nowBeat = 0; }
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
        else { awake = true; }
    }

    public void AddTime()
    { sumTime += 0.01f; }

    public void MinusTime()
    { sumTime -= 0.01f; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetronomeScript : MonoBehaviour
{
    public AudioClip clicp1, clicp2;
    AudioSource audioSource;
    public GameObject BPMInputFieldText, beatInputFieldText, starttimeInputFieldText, coeInputFieldText, timeInputFieldText, timeofloopInputFieldText;
    float bpm = 0, sumTime = 0,thestarttime=0, thecoefficient=0,timetime=0,sintime=0;
    int beat = 0, nowBeat = 0, timeofloop=0;
    bool awake = false, oftenhidden = false, once = false, looping = false, breathe = false, color = false, calc = false;
    float looptime = 100;
    public Image image;
    float alpha,beta=225/2f;
    public GameObject starttime, coefficient, loopornot, times,startstop;
    public GameObject colorchoose,timesofbreathe,deltatime;
    public GameObject[] colorpush = new GameObject[8];
    public Color gotcolor= new Color(102 / 255f, 204 / 255f, 255 / 255f, 100 / 255f);
    private void Start()
    { audioSource = this.GetComponent<AudioSource>(); }

    void Update()
    {
        sintime += Time.deltaTime;
        if (beta <= 50f) { beta = 50f;}
        if (beta > 255f) {beta = 255f;calc = false; }
        if (beta > 50f && beta < 100f) { Calc();}
        beta +=(Mathf.Sin(sintime)- Mathf.Sin(sintime-0.01f)) /0.03f;
        alpha -= 510f * Time.deltaTime* thecoefficient;
        if (timetime >= looptime) { OnButtonClick(); return; }
        if (alpha <= 0) { alpha = 0; }
        if (awake&& (oftenhidden|breathe))
        {
            timetime += Time.deltaTime;
            sumTime += Time.deltaTime;
            if (timetime < thestarttime) { image.GetComponent<Image>().color = new Color(gotcolor.r, gotcolor.g, gotcolor.b, 0 / 255f); return; }
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
        if (looping || once) { 
            image.GetComponent<Image>().color = new Color(gotcolor.r, gotcolor.g, gotcolor.b, alpha / 255f);
            once = false;
        }
        else if (timeofloop > 0)
        {
            image.GetComponent<Image>().color = new Color(gotcolor.r, gotcolor.g, gotcolor.b, beta / 255f);
        }
        else
        {
            image.GetComponent<Image>().color = new Color(102 / 255f, 204 / 255f, 255 / 255f, 0/ 255f);
        }
    }
    void Calc()
    {
        if (calc) return;
        calc = true;
        timeofloop--;
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
        if (awake) { 
            awake = false; timetime = 0;
            startstop.GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 255/ 255f);
        }
        else { 
            awake = true; once = true; sumTime += 60 / bpm;
            startstop.GetComponent<Image>().color = new Color(102 / 255f, 204 / 255f, 255 / 255f, 100 / 255f);
        }
    }

    public void AddTime()
    { sumTime += 0.01f; timetime += 0.01f; }

    public void MinusTime()
    { sumTime -= 0.01f; timetime -= 0.01f; }
    public void ChooseModeOftenhidden()
    {
        if (oftenhidden){ oftenhidden = false; ModeOftenhidden(false);once = true; }
        else { oftenhidden = true; ModeOftenhidden(true); ModeBreathe(false); breathe = false; }
    }
    public void ChooseModeBreathe()
    {
        if (breathe) { breathe = false; ModeBreathe(false); }
        else { breathe = true; ModeBreathe(true); ModeOftenhidden(false); oftenhidden = false; looping = false;once = false; }
    }
    public void ModeOftenhidden(bool state)
    {
        coefficient.SetActive(state);
        loopornot.SetActive(state);
        if (!state) { times.SetActive(false); starttime.SetActive(false); }
    }
    public void ModeBreathe(bool state)
    {
        timesofbreathe.SetActive(state);
        deltatime.SetActive(state);
    }
    public void Starttime()
    {
        try { thestarttime = float.Parse(starttimeInputFieldText.GetComponent<Text>().text)/1000; }
        catch { thestarttime = 0; }
    }
    public void ClickLoop()
    {
        if (!looping) {
            looping = true; 
            times.SetActive(true); starttime.SetActive(true); 
            loopornot.GetComponent<Image>().color = new Color(102 / 255f, 204 / 255f, 255 / 255f, 100 / 255f);
        }
        else { looping = false; 
            times.SetActive(false); starttime.SetActive(false); 
            loopornot.GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f);
        }
    }
    public void ClickCo()
    {
        try { thecoefficient = float.Parse(coeInputFieldText.GetComponent<Text>().text); }
        catch { thecoefficient = 1f; }
    }
    public void ClickTime()
    {
        if (!looping) return;
        try { looptime = float.Parse(timeInputFieldText.GetComponent<Text>().text)/1000; }
        catch { looptime = 0; }
    }
    public void Color()
    {
        if (!color) {
            color = true;
            colorchoose.GetComponent<Image>().color = new Color(102 / 255f, 204 / 255f, 255 / 255f, 100 / 255f);
        }
        else
        {
            color = false;
 
        }
    }
    public void ColorChoose(int i)
    {
        if (color)
        {
            gotcolor = colorpush[i].GetComponent<Image>().color;
            colorchoose.GetComponent<Image>().color = gotcolor;
        }
    }
    public void TimeOfLoop()
    {
        try { timeofloop = int.Parse(timeofloopInputFieldText.GetComponent<Text>().text); }
        catch { timeofloop = 2; }
    }
}

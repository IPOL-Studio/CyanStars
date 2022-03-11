using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
     //音乐
    public AudioSource music;
    public Button btnStart;
    void Start()
    {
        btnStart.onClick.AddListener(OnBtnStartClick);
    }
    void OnBtnStartClick()
    {
        music.Play();
    }
}

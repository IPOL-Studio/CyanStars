using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
public class MusicController : MonoBehaviour
{
    public static MusicController Instance;
    public AudioSource music;
    public Button btnStart;

    public Text txtTimer;

    void Start()
    {
        Instance = this;
        btnStart.onClick.AddListener(OnBtnStartClick);
    }
    void OnBtnStartClick()
    {
        music.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptToneHelper : MonoBehaviour
{
    public static PromptToneHelper Instance;

    public AudioClip ns_ka;
    public AudioClip na_ding;

    private void Start()
    {
        Instance = this;
    }

    /// <summary>
    /// ¸ù¾ÝPromptToneType·µ»ØAudioClip
    /// </summary>
    public AudioClip GetAudioClipWithType(PromptToneType type)
    {
        switch(type)
        {
            case PromptToneType.ns_ka:
                return ns_ka;

            case PromptToneType.na_ding:
                return na_ding;
        }

        return null;
    }
}

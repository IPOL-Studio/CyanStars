using System.Collections;
using System.Collections.Generic;
using CatTimeline;

/// <summary>
/// 音符轨道
/// </summary>
public class NoteTrack : BaseTrack
{
    public void OnInput(InputType inputType, InputMapData.Item item)
    {
        if (clips.Count == 0)
        {
            return;
        }

        NoteClip clip = (NoteClip) clips[0];
        clip.OnInput(inputType,item);
    }
}

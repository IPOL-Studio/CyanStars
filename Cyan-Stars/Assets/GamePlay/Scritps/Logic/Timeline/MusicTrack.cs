using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴轨道
/// </summary>
public class MusicTrack
{
    private int index;

    private TrackData data;
    
    private List<BaseNote> Notes = new List<BaseNote>();

    private List<BaseNote> needRemove = new List<BaseNote>();

    public MusicTrack(int index,TrackData data)
    {
        this.index = index;
        this.data = data;

        for (int i = 0; i < data.NoteDatas.Count; i++)
        {
            AddNote(data.NoteDatas[i]);
        }
    }
    
    /// <summary>
    /// 添加音符到轨道上
    /// </summary>
    private void AddNote(NoteData data)
    {
        BaseNote note = null;
        
        switch (data.Type)
        {
            case NoteType.Tap:
                note = new TapNote(index, data);
                break;
            case NoteType.Hold:
                note = new HoldNote(index, data);
                break;
            case NoteType.Drag:
                note = new DragNote(index,data);
                break;
            case NoteType.Click:
                note = new ClickNote(index, data);
                break;

        }
        
        Notes.Add(note);
    }

    public void OnUpdate(float deltaTime)
    {
        if (Notes.Count == 0)
        {
            return;
        }

        if (Notes[0].IsDestroyed)
        {
            Notes.RemoveAt(0);
            if (Notes.Count == 0)
            {
                return;
            }
        }
        
        foreach (BaseNote note in Notes)
        {
            note.OnUpdate(deltaTime);
        }
    }

    public void OnKeyDown()
    {
        if (Notes.Count == 0)
        {
            return;;
        }
        
        Notes[0].OnKeyDown();
    }
    
    public void OnKeyPress()
    {
        if (Notes.Count == 0)
        {
            return;;
        }
        
        Notes[0].OnKeyPress();
    }
    
    public void OnKeyUp()
    {
        if (Notes.Count == 0)
        {
            return;;
        }
        
        Notes[0].OnKeyUp();
    }




    
}

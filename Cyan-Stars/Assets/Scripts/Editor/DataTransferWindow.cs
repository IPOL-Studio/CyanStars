using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Gameplay.Camera;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Effect;
using CyanStars.Gameplay.Note;
using TMPro.Examples;
using UnityEngine;
using UnityEditor;
using EffectType = CyanStars.Gameplay.Data.EffectType;
using NoteData = CyanStars.Gameplay.Data.NoteData;

public class DataTransferWindow : EditorWindow
{
    public MusicTimelineSO so1;
    public CameraControllerSo so2;
    public EffectControllerSo so3;
    public MusicGameDataSO so4;
    [MenuItem("Tool/打开数据转换窗口")]
    public static void OpenWindow()
    {
        DataTransferWindow.GetWindow<DataTransferWindow>();
    }

    private void OnGUI()
    {
        so1 = (MusicTimelineSO)EditorGUILayout.ObjectField("",so1,typeof(MusicTimelineSO),true,null);
        so2 = (CameraControllerSo)EditorGUILayout.ObjectField("",so2,typeof(CameraControllerSo),true,null);
        so3 = (EffectControllerSo)EditorGUILayout.ObjectField("",so3,typeof(EffectControllerSo),true,null);
        so4 = (MusicGameDataSO)EditorGUILayout.ObjectField("",so4,typeof(MusicGameDataSO),true,null);

        if (GUILayout.Button("数据转换"))
        {
            so4.Data.Time = so1.musicTimelineData.Time;
            so4.Data.NoteTrackData = new NoteTrackData
            {
                BaseSpeed = so1.musicTimelineData.BaseSpeed,
                SpeedRate = so1.musicTimelineData.SpeedRate,
                LayerDatas = new List<NoteLayerData>()
            };

            for (int i = 0; i < so1.musicTimelineData.LayerDatas.Count; i++)
            {
                var layerData = so1.musicTimelineData.LayerDatas[i];
                
                NoteLayerData noteLayerData = new NoteLayerData();
                noteLayerData.TimeAxisDatas = new List<NoteTimeAxisData>();
                
                for (int j = 0; j < layerData.ClipDatas.Count; j++)
                {
                    var clipData = layerData.ClipDatas[j];
                    
                    NoteTimeAxisData noteTimeAxisData = new NoteTimeAxisData
                    {
                        NoteDatas = new List<NoteData>(),
                        SpeedRate = clipData.SpeedRate,
                        StartTime = clipData.StartTime
                    };

                    for (int k = 0; k < clipData.NoteDatas.Count; k++)
                    {
                        var oldNoteData = clipData.NoteDatas[k];
                        NoteData newNoteData = new NoteData
                        {
                            Pos = oldNoteData.Pos,
                            Type = oldNoteData.Type,
                            StartTime = oldNoteData.StartTime,
                            HoldEndTime = oldNoteData.HoldEndTime,
                            PromptToneType = oldNoteData.PromptToneType
                        };

                        noteTimeAxisData.NoteDatas.Add(newNoteData);
                    }


                    noteLayerData.TimeAxisDatas.Add(noteTimeAxisData);
                }
                
 
                
                so4.Data.NoteTrackData.LayerDatas.Add(noteLayerData);
            }

            so4.Data.CameraTrackData = new CameraTrackData
            {
                DefaultPosition = so2.defaultPosition,
                DefaultRotation = so2.defaultRotate,
                KeyFrames = new List<CameraTrackData.KeyFrame>(),
            };

            for (int i = 0; i < so2.keyFrames.Count; i++)
            {
                var keyFrame = so2.keyFrames[i];
                CameraTrackData.KeyFrame newKeyFrame = new CameraTrackData.KeyFrame
                {
                    Position = keyFrame.position,
                    Rotation = keyFrame.rotation,
                    SmoothType = keyFrame.smoothType,
                    Time = keyFrame.time,
                };
                
                
                so4.Data.CameraTrackData.KeyFrames.Add(newKeyFrame);
            }

            so4.Data.EffectTrackData = new EffectTrackData()
            {
                BPM = so3.bpm,
                KeyFrames = new List<EffectTrackData.KeyFrame>(),
            };
            for (int i = 0; i < so3.keyFrames.Count; i++)
            {
                var keyFrame = so3.keyFrames[i];

                EffectTrackData.KeyFrame newKeyFrame = new EffectTrackData.KeyFrame
                {
                    Type = keyFrame.type,
                    Time = keyFrame.time,
                    Index = keyFrame.index,
                    Position = keyFrame.position,
                    Rotation = keyFrame.rotation,
                    ParticleCount = keyFrame.particleCount,
                    Duration = keyFrame.duration,
                    Color = keyFrame.color,
                    Intensity = keyFrame.intensity,
                    Frequency = keyFrame.frequency,
                    MaxAlpha = keyFrame.maxAlpha,
                    MinAlpha = keyFrame.minAlpha,
                };

                so4.Data.EffectTrackData.KeyFrames.Add(newKeyFrame);
            }
            
            
            EditorUtility.SetDirty(so4);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CyanStars.Dialogue;

// [System.Serializable]
// class Cell
// {
//     public string sign;
//     public string id;
//     public string name;
//     public string pos;
//     public string text;
//     public string color;
//     public string link;
//     public string backgroundTex;
//     public string leftVerticalDrawing;
//     public string rightVerticalDrawing;
//     public string stop;
//     public string effect;
//     public string jump;
// }

[System.Serializable]
class Identification
{
    public string sign;
    public string id;
    public string jump;
}

[System.Serializable]
class TextContent
{
    public string name;
    public string content;
    public string link;
    public string color;
    public string stop;
}

[System.Serializable]
class VerticalDrawing
{
    public string file;
    public string xAxisMovement;
    public string effect;
    public string time;
}

[System.Serializable]
class Background
{
    public string file;
}

[System.Serializable]
class Effect
{
    public string code;
    public string parameter;
}

[System.Serializable]
class Cell
{
    public Identification identifications = new Identification();
    public TextContent textContents = new TextContent();
    public VerticalDrawing leftVerticalDrawings = new VerticalDrawing();
    public VerticalDrawing rightVerticalDrawings = new VerticalDrawing();
    public Background backgrounds = new Background();
    public Effect effects = new Effect();
}

[System.Serializable]
class Dialogue
{
    public List<Cell> dialogue = new List<Cell>();
}

public class CSV2JSON : EditorWindow
{
    public TextAsset csv;
    private Dialogue dialogue = new Dialogue();

    [MenuItem("CyanStars工具箱/CSV转JSON")]
    static void Init()
    {
        CSV2JSON window = (CSV2JSON)EditorWindow.GetWindow(typeof(CSV2JSON));
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("放入从Excel中导出的CSV", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        csv = EditorGUILayout.ObjectField("csv文件", csv, typeof(TextAsset), true) as TextAsset;
        EditorGUILayout.Space(10);

        if (GUILayout.Button("输出JOSN!"))
        {
            if (!csv)
            {
                ShowNotification(new GUIContent("没有放入文件哦~~~"));
            }
            else
            {
                CreateJSON(csv);
            }
        }
    }

/// <summary>
/// 创建JSON
/// </summary>
/// <param name="textAsset">传入UTF-8CSV</param>
    public void CreateJSON(TextAsset textAsset)
    {
        dialogue.dialogue.Clear();
        string[] rows = textAsset.text.Split('\n');
        for(int i = 2; i < rows.Length - 1; i++)
        {
            string[] cells = rows[i].Replace("\r", "").Split(',');
            Cell cell = new Cell();
            Identification identification = new Identification();
            TextContent textContent = new TextContent();
            VerticalDrawing leftVerticalDrawing = new VerticalDrawing();
            VerticalDrawing rightVerticalDrawing = new VerticalDrawing();
            Background background = new Background();
            Effect effect = new Effect();

            identification.sign = cells[0];
            identification.id = cells[1];
            identification.jump = cells[2];
            cell.identifications = identification;

            textContent.name = cells[3];
            textContent.content = cells[4];
            textContent.link = cells[5];
            textContent.color = cells[6];
            textContent.stop = cells[7];
            cell.textContents = textContent;

            leftVerticalDrawing.file = cells[8];
            leftVerticalDrawing.xAxisMovement = cells[9];
            leftVerticalDrawing.effect = cells[10];
            leftVerticalDrawing.time = cells[11];
            cell.leftVerticalDrawings = leftVerticalDrawing;

            rightVerticalDrawing.file = cells[12];
            rightVerticalDrawing.xAxisMovement = cells[13];
            rightVerticalDrawing.effect = cells[14];
            rightVerticalDrawing.time = cells[15];
            cell.rightVerticalDrawings = rightVerticalDrawing;

            background.file = cells[16];
            cell.backgrounds = background;

            effect.code = cells[17];
            effect.parameter = cells[18];
            cell.effects = effect;

            dialogue.dialogue.Add(cell);
        }

        string json = JsonUtility.ToJson(dialogue, true);
        string filepath = Application.streamingAssetsPath + "/" + csv.name + ".json";

        using (StreamWriter streamWriter = new StreamWriter(filepath))
        {
            streamWriter.WriteLine(json);
            streamWriter.Close();
            streamWriter.Dispose();
        }
        AssetDatabase.Refresh();
    }
}

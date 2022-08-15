using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CyanStars.Dialogue;

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
    public string file = null;
    public string xAxisMovement = null;
    public string effect = null;
    public string curve = null;
    public string time = null;
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
    public List<VerticalDrawing> verticalDrawings = new List<VerticalDrawing>();
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
        int verticalDrawingCount = 0;
        int index = 0;
        while ((index=rows[0].IndexOf("立绘", index)) != -1)
        {
            verticalDrawingCount++;
            index += 2;
        }

        for(int i = 2; i < rows.Length - 1; i++)
        {
            string[] cells = rows[i].Replace("\r", "").Split(',');
            Cell cell = new Cell();
            VerticalDrawing verticalDrawing = new VerticalDrawing();
            Identification identification = new Identification();
            TextContent textContent = new TextContent();
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

            int temp = 8;

            for (int j = 0; j < verticalDrawingCount; j++)
            {
                verticalDrawing = new VerticalDrawing();
                verticalDrawing.file = cells[temp];
                temp++;
                verticalDrawing.xAxisMovement = cells[temp];
                temp++;
                verticalDrawing.effect = cells[temp];
                temp++;
                verticalDrawing.curve = cells[temp];
                temp++;
                verticalDrawing.time = cells[temp];
                temp++;
                cell.verticalDrawings.Add(verticalDrawing);
            }

            background.file = cells[temp];
            temp++;
            cell.backgrounds = background;

            effect.code = cells[temp];
            temp++;
            effect.parameter = cells[temp];
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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[System.Serializable]
class Cell
{
    public string sign;
    public string id;
    public string name;
    public string pos;
    public string text;
    public string color;
    public string link;
    public string backgroundTex;
    public string leftVerticalDrawing;
    public string rightVerticalDrawing;
    public string stop;
    public string effect;
    public string jump;
}
[System.Serializable]
class TextContent
{
    public List<Cell> textContent = new List<Cell>();
}

public class CSV2JSON : EditorWindow
{
    public TextAsset csv;
    private TextContent textContent = new TextContent();

    [MenuItem("临时位置/CSV转JSON")]
    static void Init()
    {
        CSV2JSON window = (CSV2JSON)EditorWindow.GetWindow(typeof(CSV2JSON));
        window.Show();
    }

    void OnGUI()
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
        textContent.textContent.Clear();
        string[] rows = textAsset.text.Split('\n');
        for(int i = 1; i < rows.Length - 1; i++)
        {
            string[] cells = rows[i].Replace("\r", "").Split(',');
            Cell cell = new Cell();
            cell.sign = cells[0];
            cell.id = cells[1];
            cell.name = cells[2];
            cell.pos = cells[3];
            cell.text = cells[4];
            cell.color = cells[5];
            cell.link = cells[6];
            cell.backgroundTex = cells[7];
            cell.leftVerticalDrawing = cells[8];
            cell.rightVerticalDrawing = cells[9];
            cell.stop = cells[10];
            cell.effect = cells[11];
            cell.jump = cells[12];
            textContent.textContent.Add(cell);
        }

        string json = JsonUtility.ToJson(textContent, true);
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

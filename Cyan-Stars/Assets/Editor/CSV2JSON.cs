using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextContent
{
    public List<string> textContent = new List<string>();
}

public class CSV2JSON : EditorWindow
{
    public TextAsset csv;
    public TextContent textContent = new TextContent();

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

    public void CreateJSON(TextAsset textAsset)
    {
        string[] rows = textAsset.text.Split('\n');
        foreach (string row in rows)
        {
            textContent.textContent.Add(row);
        }
        // string json = JsonConvert.SerializeObject(textContent);
        string json = JsonUtility.ToJson(textContent, true);
        textContent.textContent.Clear();
        string filepath = Application.streamingAssetsPath + "/textjson.json";
        using (StreamWriter streamWriter = new StreamWriter(filepath))
        {
            streamWriter.WriteLine(json);
            streamWriter.Close();
            streamWriter.Dispose();
        }
        AssetDatabase.Refresh();
    }
}

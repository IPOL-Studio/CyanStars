using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

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
    private void CreateJSON(TextAsset textAsset)
    {
        dialogue.dialogue.Clear();
        string[] rows = textAsset.text.Split('\n');
        int verticalDrawingCount = 0;
        int index = 0;
        while ((index = rows[0].IndexOf("立绘", index)) != -1)
        {
            verticalDrawingCount++;
            index += 2;
        }

        for (int i = 2; i < rows.Length - 1; i++)
        {
            string[] cells = rows[i].Replace("\r", "").Split(',');
            Cell cell = new Cell();
            Identification identification = new Identification();
            TextContent textContent = new TextContent();
            Background background = new Background();
            Effect effect = new Effect();

            identification.sign = cells[0];
            if (identification.sign == "END")
            {
                identification.sign = "END";
                cell.identifications = identification;
                dialogue.dialogue.Add(cell);
                break;
            }
            identification.id = int.Parse(cells[1]);
            identification.jump = int.Parse(cells[2]);
            cell.identifications = identification;

            textContent.name = cells[3];
            textContent.content = cells[4];
            textContent.link = cells[5] == "是"?(byte)1:(byte)0;
            textContent.color = cells[6];
            textContent.stop = int.Parse(cells[7]);
            cell.textContents = textContent;

            int temp = 8;

            for (int j = 0; j < verticalDrawingCount; j++)
            {
                VerticalDrawing verticalDrawing = new VerticalDrawing();
                verticalDrawing.file = cells[temp];
                temp++;
                verticalDrawing.xAxisMovement = float.Parse(cells[temp]);
                temp++;
                verticalDrawing.effect = EffectComparison(cells[temp]);
                temp++;
                verticalDrawing.curve = CurveComparison(cells[temp]);
                temp++;
                verticalDrawing.time = float.Parse(cells[temp]);
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

        string json = JsonConvert.SerializeObject(dialogue, Formatting.Indented);
        string filepath = Application.streamingAssetsPath + "/Json/" + csv.name + ".json";

        using (StreamWriter streamWriter = new StreamWriter(filepath))
        {
            streamWriter.WriteLine(json);
            streamWriter.Close();
            streamWriter.Dispose();
        }

        AssetDatabase.Refresh();
    }

    private string CurveComparison(string curve)
    {
        switch (curve)
        {
            case "线性":
                return "Linear";
            case "三次方加速":
                return "InCubic";
            case "三次方减速":
                return "OutCubic";
            case "三次方加速减速":
                return "InOutCubic";
            case "指数加速":
                return "InExpo";
            case "指数减速":
                return "OutExpo";
            case "指数加速减速":
                return "InOutExpo";
            case "超范围三次方加速缓动":
                return "InBack";
            case "超范围三次方减速缓动":
                return "OutBack";
            case "超范围三次方加速减速缓动":
                return "InOutBack";
            case "指数衰减加速反弹缓动":
                return "InBounce";
            case "指数衰减减速反弹缓动":
                return "OutBounce";
            case "指数衰减加速减速反弹缓动":
                return "InOutBounce";
            default:
                return "Linear";
        }
    }

    private string EffectComparison(string effect)
    {
        switch (effect)
        {
            case "抖动":
                return "Shake";
            case "旋转抖动":
                return "ShakeRotation";
            case "缩放":
                return "ShakeScale";
            default:
                return null;
        }
    }
}

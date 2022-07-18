using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CyanStars.Dialogue
{
    // [System.Serializable]
    // public class Cell
    // {
    //     public string sign;
    //     public int id;
    //     public string name;
    //     public string pos;
    //     public string text;
    //     public string color;
    //     public string link;
    //     public string backgroundTex;
    //     public string leftVerticalDrawing;
    //     public string rightVerticalDrawing;
    //     public int stop;
    //     public string effect;
    //     public int jump;
    // }
    // [System.Serializable]
    // public class TextContent
    // {
    //     public List<Cell> textContent = new List<Cell>();
    // }
[System.Serializable]
public class Identification
{
    public string sign;
    public int id;
    public int jump;
}

[System.Serializable]
public class TextContent
{
    public string name;
    public string content;
    public string link;
    public string color;
    public int stop;
}

[System.Serializable]
public class VerticalDrawing
{
    public string file;
    public string effect;
    public string time;
}

[System.Serializable]
public class Background
{
    public string file;
}

[System.Serializable]
public class Effect
{
    public string code;
    public string parameter;
}

[System.Serializable]
public class Cell
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
    public static class AnalysisJSON
    {
        public static List<Cell> LoadJson(string dataPath)
        {
            string json;
            using (StreamReader sr = new StreamReader(dataPath))
            {
                json = sr.ReadToEnd();
                sr.Close();
            }
            List<Cell> dialogue = JsonUtility.FromJson<Dialogue>(json).dialogue;
            return dialogue;
        }
    }
}


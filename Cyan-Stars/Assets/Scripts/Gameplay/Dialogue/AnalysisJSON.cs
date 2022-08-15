using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CyanStars.Dialogue
{
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
        public float xAxisMovement;
        public string effect;
        public string curve;
        public float time;
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
        public List<VerticalDrawing> verticalDrawings = new List<VerticalDrawing>();
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


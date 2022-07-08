using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CyanStars.Framework.Dialogue
{
    [System.Serializable]
    public class Cell
    {
        public string sign;
        public int id;
        public string name;
        public string pos;
        public string text;
        public string color;
        public string link;
        public string backgroundTex;
        public string leftVerticalDrawing;
        public string rightVerticalDrawing;
        public int stop;
        public string effect;
        public int jump;
    }
    [System.Serializable]
    public class TextContent
    {
        public List<Cell> textContent = new List<Cell>();
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
            TextContent textContent = JsonUtility.FromJson<TextContent>(json);
            return textContent.textContent;
        }
    }
}


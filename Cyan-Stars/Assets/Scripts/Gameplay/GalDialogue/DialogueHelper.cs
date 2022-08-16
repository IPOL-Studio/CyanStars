using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace CyanStars.Dialogue
{
    public class DialogueHelper : MonoBehaviour
    {
        private string jsonDataFilePath;
        private List<Sprite> sprites = new List<Sprite>();

        private void Start()
        {
            DontDestroyOnLoad(this);
            jsonDataFilePath = "/Json/新格式.json";
            LoadFolderPicture(GetSprites, "/Texture");
            SceneManager.LoadScene("BundleRes/Scenes/Dialogue");
        }

        public List<Sprite> GetSprites()
        {
            return sprites;
        }

        public string GetJsonDataFilePath()
        {
            return Application.streamingAssetsPath + jsonDataFilePath;
        }

        public void GetSprites(Texture2D texture2D, string spriteName)
        {
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = spriteName;
            sprites.Add(sprite);
        }

        public void LoadFolderPicture(Action<Texture2D, string> action,string folderName)
        {
            List<string> textureFilesPath = new List<string>();
            string[] allFilesPath = Directory.GetFiles(Application.streamingAssetsPath + folderName);

            foreach(string filePath in allFilesPath)
            {
                string tmp = Path.GetExtension(filePath);
                if (tmp == ".png" || tmp == ".jpg")
                {
                    textureFilesPath.Add(filePath);
                }
            }

            foreach(string fileName in textureFilesPath)
            {
                StartCoroutine(LoadImage(action, fileName));
            }
        }

        private IEnumerator LoadImage(Action<Texture2D, string> action, string fileName)
        {
            UnityWebRequest request = UnityWebRequest.Get(fileName);
            DownloadHandlerTexture texture = new DownloadHandlerTexture(true);
            request.downloadHandler = texture;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.ConnectionError)
            {
                string[] spriteName = fileName.Split( '\\', '.');
                action(texture.texture, spriteName[spriteName.Length - 2]);
            }
        }

    }

}

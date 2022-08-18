using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueHelper : MonoBehaviour
    {
        public string jsonFilePath;
        private List<Sprite> sprites = new List<Sprite>();

        private void Start()
        {
            DontDestroyOnLoad(this);
            jsonFilePath = "/Json/新格式.json";
            LoadFolderPicture(GetSprites, "/Texture");
            SceneManager.LoadScene("BundleRes/Scenes/Dialogue");
        }

        /// <summary>
        /// 返回精灵字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Sprite> GetSpritesDictionary()
        {
            return sprites.ToDictionary(sprite => sprite.name);
        }

        /// <summary>
        /// 返回文本内容
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetDialogueContentCells()
        {
            return LoadJson(Application.streamingAssetsPath + jsonFilePath);
        }

        /// <summary>
        /// 反序列化文本内容
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        private List<Cell> LoadJson(string dataPath)
        {
            string json;
            using (StreamReader sr = new StreamReader(dataPath))
            {
                json = sr.ReadToEnd();
                sr.Close();
            }
            global::Dialogue dialogue = JsonConvert.DeserializeObject<global::Dialogue>(json);
            return dialogue?.dialogue;
        }

        /// <summary>
        /// 创建精灵并加入精灵列表
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="spriteName"></param>
        private void GetSprites(Texture2D texture2D, string spriteName)
        {
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = spriteName;
            sprites.Add(sprite);
        }

        /// <summary>
        /// 获取符合要求的图片文件地址并开始加载
        /// </summary>
        /// <param name="action"></param>
        /// <param name="folderName"></param>
        private void LoadFolderPicture(Action<Texture2D, string> action,string folderName)
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

        /// <summary>
        /// 通过地址加载图片，并创建精灵
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
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

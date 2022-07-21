using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CyanStars.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        private const string spritesScriptObjectDataPath = "Assets/BundleRes/ScriptObjects/DialogueSprites/SpritesScriptObject.asset";

        /// <summary>
        /// 精灵字典
        /// </summary>
        public Dictionary<string, Sprite> spriteDictionary { get; private set; }

        /// <summary>
        /// 当前章节的数据，从选择章节的地方传入
        /// </summary>
        public List<Cell> dialogueContentCells { get; private set; }

        public event UnityAction<int> switchDialog;

        public int dialogIndex { get; set; }

        public RectTransform rectTransform;

        #region SingletonPattern
        private static DialogueManager _instance = null;
        public static DialogueManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<DialogueManager>();
                    if(_instance == null)
                    {
                        GameObject go = new GameObject("DialogueManager");
                        _instance = go.AddComponent<DialogueManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            GetTextContent();
            spriteDictionary = InitSpriteDictionary(spritesScriptObjectDataPath);
            dialogIndex = 0;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                switchDialog?.Invoke(dialogIndex);
            }
        }

        /// <summary>
        /// 获取对话内容
        /// </summary>
        /// TODO:如果会加载大量的文本，需要拆分，就一般来讲不会这么夸张吧（大概~~~）
        private void GetTextContent()
        {
            dialogueContentCells = AnalysisJSON.LoadJson(FindObjectOfType<JsonInformation>().GetJsonDataFilePath());
        }

        /// <summary>
        /// 初始化精灵表字典
        /// </summary>
        /// <param name="dataPath">精灵表地址</param>
        /// TODO:通过读取的Json来获取精灵（如果图量很大的话）
        private Dictionary<string, Sprite> InitSpriteDictionary(string dataPath)
        {
            DialogueSpritesListObject dialogueSpritesListObject = AssetDatabase.LoadAssetAtPath<DialogueSpritesListObject>(dataPath);
            return dialogueSpritesListObject.sprites.ToDictionary(sprite => sprite.name);
        }

        public static class Colors
        {
            public const string White = "<color=white>";
            public const string Red = "<color=red>";
            public const string Yellow = "<color=yellow>";
            public const string Blue = "<color=blue>";
            public const string Green = "<color=green>";
            public const string Purple = "<color=purple>";
            public const string Gray = "<color=gray>";
            public const string Black = "<color=black>";
        }

        public static class VerticalDrawingAnimation
        {
            public const string shake = "抖动";
            public const string move = "移动";
            public const string rotate = "旋转";
        }
    }
}



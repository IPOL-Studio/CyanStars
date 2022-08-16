using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CyanStars.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        // private const string spritesScriptObjectDataPath = "Assets/BundleRes/ScriptObjects/DialogueSprites/SpritesScriptObject.asset";

        [SerializeField]
        private GameObject verticalDrawingPerfab;

        [SerializeField]
        private RectTransform verticalDrawingSpawner;

        private DialogueHelper dialogueHelper;

        /// <summary>
        /// 精灵字典
        /// </summary>
        public Dictionary<string, Sprite> spriteDictionary { get; private set; }

        /// <summary>
        /// 当前章节的数据，从选择章节的地方传入
        /// </summary>
        public List<Cell> dialogueContentCells { get; private set; }

        /// <summary>
        /// 所有的立绘，目前只在初始化的时候赋值并使用了，之后效果部分可能会用到
        /// </summary>
        public List<VerticalDrawingBase> verticalDrawingBases = new List<VerticalDrawingBase>();

        public bool initializationComplete { get; private set; }

        public event UnityAction<int> OnSwitchDialog;

        public event UnityAction<int> OnSkipAnimation;

        public event UnityAction<int> OnCreateBranchUI;

        public event UnityAction<int> OnAnimation;

        /// <summary>
        /// 当前对话ID
        /// </summary>
        [HideInInspector]
        public int dialogIndex;

        /// <summary>
        /// Canvas的RectTransform用于适配不同分辨率的立绘移动
        /// </summary>
        public RectTransform rectTransform;

        /// <summary>
        /// 动画状态计数
        /// </summary>
        public int stateCount;

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
            dialogueHelper = FindObjectOfType<DialogueHelper>();
            initializationComplete = false;
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            Init();
        }

        private void Init()
        {
            StartCoroutine(WaitForInitializationComplete());

            GetTextContent();
            GetSpriteDictionary();
            dialogIndex = 0;
            stateCount = 0;

            for (int i = 0; i < dialogueContentCells[0].verticalDrawings.Count; i++)
            {
                verticalDrawingBases.Add(Instantiate(verticalDrawingPerfab, verticalDrawingSpawner).GetComponent<VerticalDrawingBase>());
                Instance.verticalDrawingBases[i].verticalDrawingID = i;
            }
        }

        private IEnumerator WaitForInitializationComplete()
        {
            yield return new WaitForFixedUpdate();
            initializationComplete = true;
            Destroy(dialogueHelper);
        }

        /// <summary>
        /// 获取对话内容
        /// </summary>
        /// TODO:如果会加载大量的文本，进行拆分？或者其它操作？，就一般来讲不会这么夸张吧（大概~~~）
        private void GetTextContent()
        {
            dialogueContentCells = AnalysisJSON.LoadJson(dialogueHelper.GetJsonDataFilePath());
        }

        /// <summary>
        /// 初始化精灵表字典
        /// </summary>
        /// <param name="dataPath">精灵表地址</param>
        /// TODO:通过读取的Json来获取精灵（每个故事需要的图都做成一个ScriptObject，一个故事对应一个ScriptObect中包含所用的精灵）
        private void GetSpriteDictionary()
        {
            // DialogueSpritesListObject dialogueSpritesListObject = AssetDatabase.LoadAssetAtPath<DialogueSpritesListObject>(dataPath);

            spriteDictionary = dialogueHelper.GetSprites().ToDictionary(sprite => sprite.name);
        }

        // public void InvokeOnSkipAnimation(int index)
        // {
        //     OnSkipAnimation?.Invoke(index);
        // }

        public void InvokeOnSwitchDialog(int index)
        {
            OnSwitchDialog?.Invoke(index);
        }

        public void InvokeOnAnimation(int index)
        {
            OnAnimation?.Invoke(index);
        }

        public void InvokeOnCreateBranchUI(int index)
        {
            OnCreateBranchUI?.Invoke(index);
        }

        public void InvokeOnSkipAnimation(int index)
        {
            OnSkipAnimation?.Invoke(index);
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

        public static Ease AnimationEase(string curve)
        {
            switch (curve)
            {
                case "线性":
                    return Ease.Linear;
                case "三次方加速":
                    return Ease.InCubic;
                case "三次方减速":
                    return Ease.OutCubic;
                case "三次方加速减速":
                    return Ease.InOutCubic;
                case "指数加速":
                    return Ease.InExpo;
                case "指数减速":
                    return Ease.OutExpo;
                case "指数加速减速":
                    return Ease.InOutExpo;
                case "超范围三次方加速缓动":
                    return Ease.InBack;
                case "超范围三次方减速缓动":
                    return Ease.OutBack;
                case "超范围三次方加速减速缓动":
                    return Ease.InOutBack;
                case "指数衰减加速反弹缓动":
                    return Ease.InBounce;
                case "指数衰减减速反弹缓动":
                    return Ease.OutBounce;
                case "指数衰减加速减速反弹缓动":
                    return Ease.InOutBounce;
                default:
                    return Ease.Linear;
            }
        }
    }
}



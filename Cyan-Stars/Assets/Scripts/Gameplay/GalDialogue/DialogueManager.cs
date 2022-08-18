using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        // private const string spritesScriptObjectDataPath = "Assets/BundleRes/ScriptObjects/DialogueSprites/SpritesScriptObject.asset";

        [SerializeField]
        private GameObject verticalDrawingPerfab;

        [SerializeField]
        private RectTransform verticalDrawingSpawner;

        [SerializeField]
        private TMP_Text nameBoxText;

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

        // public event UnityAction<int> OnSwitchDialog;
        //
        // public event UnityAction<int> OnSkipAnimation;
        //
        // public event UnityAction<int> OnCreateBranchUI;
        //
        // public event UnityAction<int> OnAnimation;

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
            // OnSwitchDialog += SetName;
            GameRoot.Event.AddListener(DialogueEventConst.GalSwitchDialog, SetName);
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
            dialogueContentCells = dialogueHelper.GetDialogueContentCells();
        }

        /// <summary>
        /// 初始化精灵表字典
        /// </summary>
        /// <param name="dataPath">精灵表地址</param>
        /// TODO:通过读取的Json来获取精灵（每个故事需要的图都做成一个ScriptObject，一个故事对应一个ScriptObect中包含所用的精灵）
        private void GetSpriteDictionary()
        {
            spriteDictionary = dialogueHelper.GetSpritesDictionary();
        }

        private void SetName(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            if(dialogueContentCells[args.index].textContents.name == "") return;
            nameBoxText.text = dialogueContentCells[args.index].textContents.name;
        }

        public void InvokeOnSwitchDialog(int index)
        {
            // OnSwitchDialog?.Invoke(index);
            GameRoot.Event.Dispatch(DialogueEventConst.GalSwitchDialog, this, DialogueEventArgs.Create(index));
        }

        public void InvokeOnAnimation(int index)
        {
            // OnAnimation?.Invoke(index);
            GameRoot.Event.Dispatch(DialogueEventConst.GalAnimation, this, DialogueEventArgs.Create(index));
        }

        public void InvokeOnCreateBranchUI(int index)
        {
            // OnCreateBranchUI?.Invoke(index);
            GameRoot.Event.Dispatch(DialogueEventConst.GalCreateBranchUI, this, DialogueEventArgs.Create(index));
        }

        public void InvokeOnSkipAnimation(int index)
        {
            // OnSkipAnimation?.Invoke(index);
            GameRoot.Event.Dispatch(DialogueEventConst.GalSkipAnimation, this, DialogueEventArgs.Create(index));
        }

        public static Ease AnimationEase(string curve)
        {
            return Enum.TryParse<Ease>(curve, out var result) ? result : Ease.Linear;
        }
    }
}



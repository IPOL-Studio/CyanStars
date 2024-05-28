using System;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MapSelectionUI/MapSelectionPanelRework.prefab")]
    public class MapSelectionPanelR : BaseUIPanel
    {
        [SerializeField]
        private Toggle autoModeToggle;
        [SerializeField]
        private Button backButton;

        public StarControllerR StarController;

        private Dictionary<Type, IMapSelectionPage> pageDict;
        private Stack<IMapSelectionPage> pageStack = new Stack<IMapSelectionPage>();

        public MapItemData CurrentSelectedMap { get; set; }

        public void ChangePage<T>() where T : IMapSelectionPage
        {
            if (!pageDict.TryGetValue(typeof(T), out IMapSelectionPage page))
            {
                Debug.LogWarning("page can not be null");
                return;
            }

            var args = new MapSelectionPageChangeArgs()
            {
                FadeTime = 1.5f,
                AnimationEase = Ease.OutQuart
            };

            var currentPage = pageStack.Count > 0 ? pageStack.Peek() : null;

            currentPage?.OnExit(args);

            currentPage = page;
            pageStack.Push(currentPage);
            currentPage.OnEnter(args);
        }

        public bool IsActive<T>() where T : IMapSelectionPage
        {
            return pageStack.Count > 0 && pageStack.Peek().GetType() == typeof(T);
        }

        protected override void OnCreate()
        {
            var musicGameModule = GameRoot.GetDataModule<MusicGameModule>();
            CurrentSelectedMap = MapItemData.Create(musicGameModule.MapIndex, musicGameModule.GetMaps()[musicGameModule.MapIndex]);

            var pages = this.GetComponentsInChildren<IMapSelectionPage>(true);
            pageDict = new Dictionary<Type, IMapSelectionPage>();

            foreach (var page in pages)
            {
                page.OnInit(this);
                pageDict.Add(page.GetType(), page);
                ((Component)page).gameObject.SetActive(false);
            }

            autoModeToggle.onValueChanged.AddListener((isOn) =>
                GameRoot.GetDataModule<MusicGameModule>().IsAutoMode = isOn
            );

            backButton.onClick.AddListener(() =>
            {
                if (pageStack.Count > 1)
                {
                    var args = new MapSelectionPageChangeArgs()
                    {
                        FadeTime = 1.5f,
                        AnimationEase = Ease.OutQuart
                    };

                    pageStack.Pop().OnExit(args);
                    pageStack.Peek().OnEnter(args);
                }
                else if (pageStack.Count == 1)
                {

                }
            });
        }

        public override void OnOpen()
        {
            ChangePage<MapListPage>();
            StarController.GenerateStars();
        }

        private void Update()
        {
            StarController.OnUpdate();
        }
    }

    public interface IMapSelectionPage
    {
        void OnInit(MapSelectionPanelR owner);
        void OnEnter(MapSelectionPageChangeArgs args);
        void OnExit(MapSelectionPageChangeArgs args);
    }

    public struct MapSelectionPageChangeArgs
    {
        public float FadeTime;
        public Ease AnimationEase;
    }
}

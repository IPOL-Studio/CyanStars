#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataLinkItemView : BaseView<ChartPackDataLinkItemViewModel>
    {
        [Header("Item 自身")]
        [SerializeField]
        private Toggle iconToggle = null!;

        [SerializeField]
        private Image iconImage = null!;

        [SerializeField]
        private TMP_InputField linkButtonLabelInputField = null!;

        [SerializeField]
        private TMP_InputField linkButtonUrlInputField = null!;

        [SerializeField]
        private Button deleteLinkItemButton = null!;


        [Header("下拉菜单模板")]
        [SerializeField]
        private GameObject dropdownPopupGameObject = null!;

        [SerializeField]
        private LinkDropdownItem linkDropdownItem = null!;

        [SerializeField]
        private List<LinkDropdownItemData> linkDropdownItemDatas = new();


        public override void Bind(ChartPackDataLinkItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.LinkIcon
                .Subscribe(async linkIcon =>
                {
                })
                .AddTo(this);
            ViewModel.LinkTitle
                .Subscribe(linkTitle =>
                {
                })
                .AddTo(this);
            ViewModel.LinkUrl
                .Subscribe(linkUrl =>
                {
                })
                .AddTo(this);
        }
    }

    [Serializable]
    internal class LinkDropdownItemData
    {
        public LinkIcon? LinkIconType;
        public Sprite? IconSprite;
        public string IconDescription = "";
    }
}

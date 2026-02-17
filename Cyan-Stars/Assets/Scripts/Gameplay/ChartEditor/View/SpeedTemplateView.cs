#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class SpeedTemplateView : BaseView<SpeedTemplateViewModel>
    {
        [SerializeField]
        private GameObject speedTemplateItemPrefab = null!;

        [SerializeField]
        private Transform listContentTransform = null!;

        [SerializeField]
        private Button addSpeedTemplateItemButton = null!;


        public override void Bind(SpeedTemplateViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            // 初始化时实例化已有的 item
            int index = 0;
            foreach (var vm in ViewModel.SpeedTemplateData)
            {
                var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                go.GetComponent<SpeedTemplateListItemView>().Bind(vm);
                go.transform.SetSiblingIndex(index);
                index++;
            }

            ViewModel.SpeedTemplateData.ObserveAdd()
                .Subscribe(e =>
                    {
                        var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                        go.GetComponent<SpeedTemplateListItemView>().Bind(e.Value.View);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                )
                .AddTo(this);
            ViewModel.SpeedTemplateData.ObserveRemove()
                .Subscribe(e =>
                    {
                        var itemToRemove = listContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);
                    }
                )
                .AddTo(this);

            addSpeedTemplateItemButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddSpeedTemplateData())
                .AddTo(this);
        }
    }
}

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.ViewModel;
using CyanStars.ChartEditor.View;
using UnityEngine;

namespace CyanStars.ChartEditor
{
    public class MvvmManager : MonoBehaviour
    {
        public GameObject MainCanva;

        private MainModel model;
        private MainViewModel viewModel;
        private BaseView[] views;

        private void Awake()
        {
            model = new MainModel();
            model.CreateChartPack(title: "New Chart Pack");
            model.CreateChart();
            viewModel = new MainViewModel(model);
            views = MainCanva.GetComponentsInChildren<BaseView>();
        }

        private void Start()
        {
            foreach (BaseView view in views)
            {
                view.Bind(viewModel);
            }
        }
    }
}

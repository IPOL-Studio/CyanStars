using System.Collections.Generic;
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

using CyanStars.ChartEditor.ViewModel;
using UnityEngine;

namespace CyanStars.ChartEditor.View
{
    public abstract class BaseView : MonoBehaviour
    {
        public abstract void Bind(MainViewModel viewModel);
    }
}

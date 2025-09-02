using CyanStars.ChartEditor.Model;
using UnityEngine;

namespace CyanStars.ChartEditor.View
{
    public class BaseView : MonoBehaviour
    {
        protected EditorModel Model;

        public virtual void Band(EditorModel editorModel)
        {
            Model = editorModel;
        }
    }
}

#nullable enable

using CyanStars.Framework;
using CyanStars.Framework.Event;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor
{
    public class Background : MonoBehaviour, IPointerClickHandler
    {
        public const string ClickEventName = "ChartEditorBackgroundClick";

        public void OnPointerClick(PointerEventData eventData)
        {
            GameRoot.Event.Dispatch(ClickEventName, this, EmptyEventArgs.Create());
        }
    }
}

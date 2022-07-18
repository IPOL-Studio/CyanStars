using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class LeftVerticalDrawing : Image, ISetSprites
    {
        protected override void Start()
        {
            DialogueManager.Instance.switchDialog += SetImage;
        }

        public void SetImage(int index)
        {
            if(DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file];
        }
    }
}


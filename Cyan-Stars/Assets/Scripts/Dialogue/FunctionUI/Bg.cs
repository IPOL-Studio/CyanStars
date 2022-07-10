using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class Bg : Image, ISetSprites
    {
        protected override void Start()
        {
            DialogueManager.Instance.switchDialog += SetImage;
        }

        public void SetImage(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].backgroundTex == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[
                DialogueManager.Instance.dialogueContentCells[index].backgroundTex];
        }
    }
}


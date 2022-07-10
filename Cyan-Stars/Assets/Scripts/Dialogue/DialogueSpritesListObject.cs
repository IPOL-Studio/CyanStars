using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Dialogue
{
    [CreateAssetMenu(menuName = "创建精灵列表")]
    public class DialogueSpritesListObject : ScriptableObject
    {
        public List<Sprite> sprites;
    }
}


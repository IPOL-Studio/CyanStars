using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Utils
{
    public static class Texture2DExtension
    {
        /// <summary>
        /// 将Texture2D转换为Sprite
        /// </summary>
        public static Sprite ConvertToSprite(this Texture2D self)
        {
            Sprite result = Sprite.Create(self,new Rect(0,0,self.width,self.height),new Vector2(0.5f,0.5f));
            return result;
        }
    }
}


using System.Text;
using CyanStars.Framework;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueDataModule : BaseDataModule
    {
        public StringBuilder Content { get; } = new StringBuilder(64);

        /// <summary>
        /// 指示 Content 当前是否被修改过
        /// </summary>
        public bool IsContentDirty { get; set; }

        public override void OnInit()
        {

        }
    }
}

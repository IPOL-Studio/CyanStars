using System;

namespace CyanStars.Framework.Dialogue
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DialogueNodeAttribute : Attribute
    {
        public string NodeType { get; }

        public DialogueNodeAttribute(string nodeType)
        {
            NodeType = nodeType;
        }
    }
}

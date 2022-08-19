using System;
using System.Collections.Generic;
using System.Reflection;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueMetadataModule : BaseDataModule
    {
        //Node type key => Node
        private readonly Dictionary<string, Type> NodeDict = new Dictionary<string, Type>();

        //Step type key => Step
        private readonly Dictionary<string, Type> StepDict = new Dictionary<string, Type>();

        private readonly Dictionary<Type, DialogueStepAttribute> AttrDict = new Dictionary<Type, DialogueStepAttribute>();


        public override void OnInit()
        {
            Type[] types = GetType().Assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BaseStep)))
                {
                    var attr = type.GetCustomAttribute<DialogueStepAttribute>();
                    if (attr != null)
                    {
                        StepDict.Add(attr.StepType, type);
                        AttrDict.Add(type, attr);
                    }
                }
                else if (type.IsSubclassOf(typeof(BaseNode)))
                {
                    var attr = type.GetCustomAttribute<DialogueNodeAttribute>();
                    if (attr != null)
                    {
                        NodeDict.Add(attr.NodeType, type);
                    }
                }
            }
        }

        public Type GetStepType(string typeKey)
        {
            return StepDict.TryGetValue(typeKey, out var type) ? type : null;
        }

        public DialogueStepAttribute GetDialogueStepAttribute<T>() where T : BaseStep
        {
            return GetDialogueStepAttribute(typeof(T));
        }

        public DialogueStepAttribute GetDialogueStepAttribute(Type type)
        {
            return AttrDict.TryGetValue(type, out var attr) ? attr : null;
        }

        public Type GetNodeType(string typeKey)
        {
            return NodeDict.TryGetValue(typeKey, out var type) ? type : null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueMetadataModule : BaseDataModule
    {
        //Node type key => Node Type
        private readonly Dictionary<string, Type> NodeDict = new Dictionary<string, Type>();

        //ActionUnit type key => ActionUnit Type
        private readonly Dictionary<string, Type> ActionUnitDict = new Dictionary<string, Type>();

        private readonly Dictionary<Type, DialogueActionUnitAttribute> AttrDict = new Dictionary<Type, DialogueActionUnitAttribute>();


        public override void OnInit()
        {
            Type[] types = GetType().Assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BaseActionUnit)))
                {
                    var attr = type.GetCustomAttribute<DialogueActionUnitAttribute>();
                    if (attr != null)
                    {
                        ActionUnitDict.Add(attr.ActionType, type);
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

        public Type GetActionUnitType(string typeKey)
        {
            return ActionUnitDict.TryGetValue(typeKey, out var type) ? type : null;
        }

        public DialogueActionUnitAttribute GetDialogueActionUnitAttribute<T>() where T : BaseActionUnit
        {
            return GetDialogueActionUnitAttribute(typeof(T));
        }

        public DialogueActionUnitAttribute GetDialogueActionUnitAttribute(Type type)
        {
            return AttrDict.TryGetValue(type, out var attr) ? attr : null;
        }

        public Type GetNodeType(string typeKey)
        {
            return NodeDict.TryGetValue(typeKey, out var type) ? type : null;
        }
    }
}

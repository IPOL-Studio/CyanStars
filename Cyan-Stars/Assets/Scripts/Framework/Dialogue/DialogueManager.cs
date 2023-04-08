using System.Collections;
using System.Collections.Generic;
using MunNovel;
using UnityEngine;

namespace CyanStars.Framework.Dialogue
{
    public class DialogueManager : BaseManager
    {
        public override int Priority { get; }

        public ServiceManager ServiceManager { get; private set; }


        public override void OnInit()
        {
            ServiceManager = new ServiceManager();
            ServiceManager.SetInstanceProvider(() => ServiceManager);

            CommandManager commandManager = new CommandManager();
            ServiceManager.Register(commandManager);
        }

        public override void OnUpdate(float deltaTime)
        {

        }
    }
}

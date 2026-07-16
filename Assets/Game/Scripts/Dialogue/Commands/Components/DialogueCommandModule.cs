using Game.Commands;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Dialogue.Commands
{
    [DisallowMultipleComponent]
    public sealed class DialogueCommandModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterCommandRoutes<
                DialogueStartRoutes>();

            builder.RegisterCommandRoute<
                DialogueStartRoutes,
                StartDialogueCommand,
                DialogueStartResult>();
        }
    }
}
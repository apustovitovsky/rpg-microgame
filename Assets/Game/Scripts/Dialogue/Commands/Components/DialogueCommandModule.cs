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
            builder.Register<StartDialogueCommandHandler>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
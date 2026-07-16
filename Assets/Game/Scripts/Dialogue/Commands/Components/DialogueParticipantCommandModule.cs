using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Dialogue.Commands
{
    [DisallowMultipleComponent]
    public sealed class DialogueParticipantCommandModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<DialogueParticipantSessionStore>(
                Lifetime.Scoped);

            builder.Register<EnterDialogueCommandHandler>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<ExitDialogueCommandHandler>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
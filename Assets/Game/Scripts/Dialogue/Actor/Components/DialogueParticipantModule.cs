using Game.Core;
using UnityEngine;
using VContainer;


namespace Game.Dialogue.Actor
{
    [DisallowMultipleComponent]
    public sealed class DialogueParticipantModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<DialogueParticipant>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
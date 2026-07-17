using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Dialogue.Actor
{
    [DisallowMultipleComponent]
    public sealed class DialogueParticipantModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField]
        private DialogueParticipation _participation;

        public void Install(
            IContainerBuilder builder)
        {
            if (_participation == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DialogueParticipantModule)} requires " +
                    $"a {nameof(DialogueParticipation)}.");
            }

            builder.RegisterComponent(_participation)
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Register<DialogueParticipant>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
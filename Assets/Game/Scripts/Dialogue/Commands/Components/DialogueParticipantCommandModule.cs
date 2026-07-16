using Cysharp.Threading.Tasks;
using Game.Commands;
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
            builder.RegisterCommandExecutionGroup<
                DialogueParticipantExecution>();

            builder.RegisterCommandExecution<
                DialogueParticipantExecution,
                EnterDialogueSessionCommand,
                IUniTaskAsyncDisposable>();
        }
    }
}
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Dialogue.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerDialogueModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<PlayerDialogueLifecycle>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
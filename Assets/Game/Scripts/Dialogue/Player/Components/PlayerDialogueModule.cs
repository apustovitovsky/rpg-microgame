using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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
            builder.RegisterEntryPoint<
                PlayerDialogueInputController>(
                Lifetime.Scoped);
        }
    }
}
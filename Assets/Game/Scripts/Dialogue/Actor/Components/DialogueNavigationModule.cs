using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Dialogue.Actor
{
    [DisallowMultipleComponent]
    public sealed class DialogueNavigationModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<DialogueNavigationLifecycle>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public readonly struct InstallContext
    {
        public LifetimeScope Scope { get; }
        public IContainerBuilder Builder { get; }

        public InstallContext(LifetimeScope scope, IContainerBuilder builder)
        {
            Scope = scope;
            Builder = builder;
        }
    }
}
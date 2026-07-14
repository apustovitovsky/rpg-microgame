using System;
using VContainer;
using VContainer.Unity;

namespace Game.World
{
    public sealed class WorldInstanceInstaller<TInstance> :
        IInstaller
        where TInstance : WorldInstance
    {
        private readonly TInstance _instance;

        public WorldInstanceInstaller(TInstance instance)
        {
            _instance = instance
                ?? throw new ArgumentNullException(nameof(instance));
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_instance)
                .AsSelf()
                .As<WorldInstance>()
                .AsImplementedInterfaces();
        }
    }
}
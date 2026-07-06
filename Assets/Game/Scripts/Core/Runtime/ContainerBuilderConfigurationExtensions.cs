using UnityEngine;
using VContainer;

namespace Game.Core
{
    public static class ContainerBuilderConfiguratorExtensions
    {
        public static void Configure(
            this IContainerBuilder builder,
            BuildConfiguratorSO configuration)
        {
            if (configuration == null)
                return;

            configuration.Install(builder);
        }
    }
}
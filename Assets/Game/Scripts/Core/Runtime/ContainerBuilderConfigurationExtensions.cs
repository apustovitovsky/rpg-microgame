using VContainer;

namespace Game.Core
{
    public static class ContainerBuilderConfigurationExtensions
    {
        public static void Configure(
            this IContainerBuilder builder,
            BuildConfigurationSO configuration)
        {
            if (configuration == null)
                return;

            configuration.Install(builder);
        }
    }
}
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public static class ContainerBuilderComponentExtensions
    {
        public static ComponentRegistrationBuilder RegisterComponentInScope<T>(
            this IContainerBuilder builder)
        {
            return builder
                .RegisterComponentInHierarchy<T>()
                .UnderTransform(resolver =>
                    resolver.Resolve<ScopeRoot>().Transform);
        }
    }
}
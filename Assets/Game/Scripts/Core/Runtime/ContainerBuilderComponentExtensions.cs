using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public static class ContainerBuilderComponentExtensions
    {
        public static ComponentRegistrationBuilder RegisterComponentInModuleRoot<T>(
            this IContainerBuilder builder)
        {
            return builder
                .RegisterComponentInHierarchy<T>()
                .UnderTransform(resolver =>
                    resolver.Resolve<ModuleRoot>().Transform);
        }
    }
}
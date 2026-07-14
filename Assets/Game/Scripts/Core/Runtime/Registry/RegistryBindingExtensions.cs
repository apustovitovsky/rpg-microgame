using System;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public static class RegistryBindingExtensions
    {
        public static void RegisterBinding<T>(
            this IContainerBuilder builder)
            where T : class
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterEntryPoint<RegistryBinding<T>>(
                Lifetime.Scoped);
        }
    }
}
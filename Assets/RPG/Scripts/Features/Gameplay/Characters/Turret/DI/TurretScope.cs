using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class TurretScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseComponents(transform, components =>
            {
                components.AddInHierarchy<CharacterController>();
            });

            builder.Register<CharacterGravityService>(Lifetime.Scoped)
                .As<ICharacterGravityService>();

            builder.RegisterEntryPoint<TurretController>()
                .As<IPlayerInputHandler>();
        }
    }
}

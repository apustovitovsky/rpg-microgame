using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PlayerScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseComponents(transform, components =>
            {
                components.AddInHierarchy<CharacterController>();
            });

            builder.Register<CharacterGravityService>(Lifetime.Singleton)
                .As<ICharacterGravityService>();

            builder.RegisterEntryPoint<PlayerController>()
                .As<IPlayerInputHandler>();
        }
    }
}

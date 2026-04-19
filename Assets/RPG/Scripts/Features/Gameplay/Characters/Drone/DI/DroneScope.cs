using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class DroneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseComponents(transform, components =>
            {
                components.AddInHierarchy<CharacterController>();
            });

            builder.RegisterEntryPoint<DroneController>()
                .As<IPlayerInputHandler>();
        }
    }
}

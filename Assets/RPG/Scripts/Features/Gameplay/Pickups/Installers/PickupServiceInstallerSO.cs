using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupServiceInstaller", menuName = "RPG/Gameplay/Pickup/Pickup Service Installer")]
    public sealed class PickupServiceInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<PickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<GroundPickup>()

                .UnderTransform(context.Scope.transform)
                .WithParameter<IPickup>(new HealthPickup(10));
        }
    }
}

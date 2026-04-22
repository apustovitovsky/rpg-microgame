using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupInstaller", menuName = "RPG/Gameplay/Pickup/Pickup Installer")]
    public sealed class PickupInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<PickupCollectionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterFactory<PickupDefinitionSO, WorldPickup, IPickupInstance>(
                (definition, worldPickup) => new PickupInstance(definition, worldPickup));

            builder.RegisterComponentInHierarchy<WorldPickup>()
                .UnderTransform(context.Scope.transform);
        }
    }
}

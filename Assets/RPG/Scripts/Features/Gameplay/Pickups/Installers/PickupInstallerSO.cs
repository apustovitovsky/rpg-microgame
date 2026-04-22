using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupInstaller", menuName = "RPG/Gameplay/Pickup/Pickup Installer")]
    public sealed class PickupInstallerSO : InstallerSO
    {
        [SerializeField] private PickupDefinitionSO _pickupDefinition;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<PickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterFactory<PickupDefinitionSO, IPickupInstance>(definition => new PickupInstance(definition));

            builder.RegisterComponentInHierarchy<GroundPickup>()
                .UnderTransform(context.Scope.transform)
                .WithParameter(_pickupDefinition);
        }
    }
}

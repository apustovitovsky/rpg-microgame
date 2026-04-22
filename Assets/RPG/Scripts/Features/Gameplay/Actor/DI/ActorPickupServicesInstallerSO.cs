using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorPickupServicesInstaller", menuName = "RPG/Gameplay/Pickup/Actor Pickup Services Installer")]
    public sealed class ActorPickupServicesInstallerSO : InstallerSO
    {
        [Header("Parameters")]

        [Tooltip("Amount of actor health")]
        [SerializeField] private float curHealth = 25f;
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Amount of actor gold")]
        [SerializeField] private int curGold = 5;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<ActorHealth>(Lifetime.Scoped)
                .WithParameter(nameof(curHealth), curHealth)
                .WithParameter(nameof(maxHealth), maxHealth)
                .AsImplementedInterfaces();

            builder.Register<ActorInventory>(Lifetime.Scoped)
                .WithParameter(curGold)
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<ActorPickupCollector>()
                .UnderTransform(context.Scope.transform);
        }
    }
}

using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorHealthInstaller", menuName = "RPG/Gameplay/Installers/Actor Health Installer")]
    public sealed class ActorHealthInstallerSO : InstallerSO
    {
        [SerializeField] private float maxHealth = 100f;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<ActorHealth>(Lifetime.Scoped)
                .WithParameter(maxHealth);

            builder.RegisterComponentInHierarchy<ActorPickupReceiver>()
                .UnderTransform(context.Scope.transform);
        }
    }
}

using RPG.Core;
using UnityEngine;
using VContainer;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorGravityInstaller", menuName = "RPG/Gameplay/Installers/Actor Gravity Installer")]
    public class ActorGravityInstallerSO : InstallerSO
    {
        [SerializeField] private float gravityForce = -9.81f;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.Register<ActorGravityService>(Lifetime.Scoped)
                   .WithParameter(gravityForce)
                   .AsImplementedInterfaces();
        }
    }
}
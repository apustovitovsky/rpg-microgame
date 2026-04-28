using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Actor
{
    [CreateAssetMenu(fileName = "ActorGravityInstaller", menuName = "Etheria/Gameplay/Installers/Actor Gravity Installer")]
    public class ActorGravityInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private float gravityForce = -9.81f;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<ActorGravityService>(Lifetime.Scoped)
                .WithParameter(gravityForce)
                .AsImplementedInterfaces();
        }
    }
}


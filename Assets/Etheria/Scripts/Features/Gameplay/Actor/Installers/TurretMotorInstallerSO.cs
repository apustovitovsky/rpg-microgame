using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "TurretMotorInstaller", menuName = "Etheria/Gameplay/Installers/Turret Motor Installer")]
    public class TurretMotorInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<TurretMotor>()
                .As<IActorInputHandler>();
        }
    }
}


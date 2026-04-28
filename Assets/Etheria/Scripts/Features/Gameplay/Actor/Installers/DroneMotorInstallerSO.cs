using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(fileName = "DroneMotorInstaller", menuName = "Etheria/Gameplay/Installers/Drone Motor Installer")]
    public class DroneMotorInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<DroneMotor>()
                .As<IActorInputHandler>();
        }
    }
}

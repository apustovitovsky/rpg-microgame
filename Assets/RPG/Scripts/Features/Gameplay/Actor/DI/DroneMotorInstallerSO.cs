using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "DroneMotorInstaller", menuName = "RPG/Gameplay/Installers/Drone Motor Installer")]
    public class DroneMotorInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DroneMotor>()
                .As<IActorInputHandler>();
        }
    }
}

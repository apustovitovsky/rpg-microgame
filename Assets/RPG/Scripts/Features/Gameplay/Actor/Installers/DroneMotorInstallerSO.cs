using RPG.Core;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "DroneMotorInstaller", menuName = "RPG/Gameplay/Installers/Drone Motor Installer")]
    public class DroneMotorInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterEntryPoint<DroneMotor>()
                .As<IActorInputHandler>();
        }
    }
}
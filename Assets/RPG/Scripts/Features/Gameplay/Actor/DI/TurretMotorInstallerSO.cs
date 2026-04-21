using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "TurretMotorInstaller", menuName = "RPG/Gameplay/Installers/Turret Motor Installer")]
    public class TurretMotorInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterEntryPoint<TurretMotor>()
                .As<IActorInputHandler>();
        }
    }
}
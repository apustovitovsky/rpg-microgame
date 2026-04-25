using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "TurretMotorInstaller", menuName = "RPG/Gameplay/Installers/Turret Motor Installer")]
    public class TurretMotorInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<TurretMotor>()
                .As<IActorInputHandler>();
        }
    }
}

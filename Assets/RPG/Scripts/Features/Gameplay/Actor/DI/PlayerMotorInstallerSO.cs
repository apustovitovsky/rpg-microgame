using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PlayerMotorInstaller", menuName = "RPG/Gameplay/Installers/Player Motor Installer")]
    public class PlayerMotorInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterEntryPoint<PlayerMotor>()
                .As<IActorInputHandler>();
        }
    }
}
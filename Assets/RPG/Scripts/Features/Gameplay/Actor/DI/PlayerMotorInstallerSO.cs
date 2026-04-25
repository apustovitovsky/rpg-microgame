using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PlayerMotorInstaller", menuName = "RPG/Gameplay/Installers/Player Motor Installer")]
    public class PlayerMotorInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<PlayerMotor>()
                .As<IActorInputHandler>();
        }
    }
}

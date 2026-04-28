using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(fileName = "PlayerMotorInstaller", menuName = "Etheria/Gameplay/Installers/Player Motor Installer")]
    public class PlayerMotorInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<PlayerMotor>()
                .As<IActorInputHandler>();
        }
    }
}

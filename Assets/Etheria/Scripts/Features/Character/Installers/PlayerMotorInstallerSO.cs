using Etheria.Core.DI;
using Etheria.Game.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(fileName = "PlayerMotorInstaller", menuName = "Etheria/Gameplay/Installers/Player Motor Installer")]
    public class PlayerMotorInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<PlayerMotor>()
                .As<IActorInputHandler>()
                .As<IActorFacingHandler>();
        }
    }
}

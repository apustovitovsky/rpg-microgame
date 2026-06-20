using Etheria.Core.DI;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Player
{
    [CreateAssetMenu(
        fileName = "PlayerTargetServiceInstaller",
        menuName = "Etheria/Features/Player/Player Target Service Installer")]
    public sealed class PlayerTargetServiceInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<PlayerTargetService>(Lifetime.Singleton)
                .As<IPlayerTargetProvider>()
                .As<IPlayerTargetService>();
        }
    }
}
using System;
using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    [CreateAssetMenu(
        fileName = "PlayerInteractionInstaller",
        menuName = "Etheria/Features/Player/Player Interaction Installer")]
    public sealed class PlayerInteractionInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<PlayerInteractionService>(
                Lifetime.Singleton);
        }
    }
}
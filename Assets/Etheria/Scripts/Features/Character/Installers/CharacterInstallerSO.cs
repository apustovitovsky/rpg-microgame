using Etheria.Core.DI;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterInstaller",
        menuName = "Etheria/Features/Character/Character Installer")]
    public class CharacterInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CharacterInteractionService>(
                Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<CharacterTargetSensor>()
                .UnderScopeRoot()
                .As<ITargetCandidateSource>();

            builder.RegisterComponentInHierarchy<PlayerCameraLookController>()
                .UnderScopeRoot();

            builder.RegisterComponentInHierarchy<PlayerCharacterAnimationController>()
                .UnderScopeRoot();

            builder.RegisterEntryPoint<CharacterTargetingService>(Lifetime.Scoped)
                .As<ITargetProvider>();

            builder.RegisterEntryPoint<CharacterLabelPresenter>(Lifetime.Scoped);
        }
    }
}

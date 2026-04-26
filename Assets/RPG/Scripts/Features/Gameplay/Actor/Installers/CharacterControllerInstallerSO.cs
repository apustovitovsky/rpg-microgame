using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "CharacterControllerInstaller", menuName = "RPG/Gameplay/Installers/Character Controller Installer")]
    public class CharacterControllerInstallerSO : ScopeInstallerSO
    {
        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<CharacterController>()
                .UnderTransform(scope.transform);
        }
    }
}

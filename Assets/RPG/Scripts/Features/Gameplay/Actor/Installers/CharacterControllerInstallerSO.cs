using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "CharacterControllerInstaller", menuName = "RPG/Gameplay/Installers/Character Controller Installer")]
    public class CharacterControllerInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterComponentInHierarchy<CharacterController>()
                .UnderTransform(context.Scope.transform);

        }
    }
}
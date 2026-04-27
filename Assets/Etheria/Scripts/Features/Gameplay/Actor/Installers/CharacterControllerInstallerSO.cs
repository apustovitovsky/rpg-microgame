using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(fileName = "CharacterControllerInstaller", menuName = "Etheria/Gameplay/Installers/Character Controller Installer")]
    public class CharacterControllerInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterComponentInHierarchy<CharacterController>()
                .UnderTransform(rootObject.transform);
        }
    }
}

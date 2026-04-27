// using UnityEngine;
// using VContainer;

// namespace Etheria.Core.DI
// {
//     [CreateAssetMenu(
//         fileName = "ModularInstaller",
//         menuName = "Etheria/Core/Modular Installer")]
//     public sealed class ModularInstallerSO : InstallerSO
//     {
//         [SerializeField] private InstallerSO[] _installers;

//         public override void Install(IContainerBuilder builder, SceneScopeContext context)
//         {
//             if (_installers == null)
//                 return;

//             foreach (var installer in _installers)
//             {
//                 if (installer == null)
//                     continue;

//                 installer.Install(builder, context);
//             }
//         }
//     }
// }

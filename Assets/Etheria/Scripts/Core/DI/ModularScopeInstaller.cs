// using UnityEngine;
// using VContainer;
// using VContainer.Unity;

// namespace Etheria.Core.DI
// {
//     public sealed class ModularScopeInstaller : ScopeInstallerSO
//     {
//         [SerializeField] private InstallerSO[] _installers;
//         [SerializeField] private ScopeInstallerSO[] _scopeInstallers;

//         public override void Install(LifetimeScope scope, IContainerBuilder builder)
//         {
//             var context = new SceneScopeContext(
//                 definition: null,
//                 root: scope.gameObject,
//                 scope: scope);

//             if (_installers != null)
//             {
//                 foreach (var installer in _installers)
//                 {
//                     if (installer == null)
//                         continue;

//                     installer.Install(builder, context);
//                 }
//             }

//             if (_scopeInstallers == null)
//                 return;

//             foreach (var installer in _scopeInstallers)
//             {
//                 if (installer == null)
//                     continue;

//                 installer.Install(scope, builder);
//             }
//         }
//     }
// }

// using UnityEngine;
// using VContainer;
// using VContainer.Unity;

// namespace Etheria.Core.DI
// {
//     public sealed class ModularScope : LifetimeScope
//     {
//         [SerializeField] private InstallerSO[] _installers;
//         [SerializeField] private ScopeInstallerSO[] _scopeInstallers;

//         protected override void Configure(IContainerBuilder builder)
//         {
//             var context = new SceneScopeContext(
//                 definition: null,
//                 root: gameObject,
//                 scope: this);

//             builder.RegisterInstance(context);

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

//                 installer.Install(this, builder);
//             }
//         }
//     }
// }

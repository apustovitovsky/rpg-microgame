// using System;
// using UnityEngine;
// using VContainer;
// using VContainer.Unity;

// namespace Etheria.Core.DI
// {
//     public abstract class ScopeInstallerSO : InstallerSO
//     {
//         public sealed override void Install(IContainerBuilder builder, SceneScopeContext context)
//         {
//             if (context?.Scope == null)
//             {
//                 throw new InvalidOperationException(
//                     $"{GetType().Name} requires a {nameof(LifetimeScope)} in the current {nameof(SceneScopeContext)}.");
//             }

//             Install(context.Scope, builder);
//         }

//         public abstract void Install(LifetimeScope scope, IContainerBuilder builder);
//     }
// }

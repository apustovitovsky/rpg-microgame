// using UnityEngine;
// using VContainer;

// namespace Etheria.Core.DI
// {
//     [CreateAssetMenu(
//         fileName = "TestInstaller",
//         menuName = "Etheria/Core/Scene Loading/Test Installer")]
//     public sealed class TestInstallerSO : InstallerSO
//     {
//         public override void Install(IContainerBuilder builder, SceneScopeContext context)
//         {
//             var root = context.Root;
//             Debug.Log($"TestInstallerSO.Install() for scene: {context.Definition.ScenePath}. Root: {root.name}.");
//         }
//     }
// }

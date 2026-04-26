using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    [CreateAssetMenu(
    fileName = "TestScopeInstaller",
    menuName = "RPG/Core/Scene Loading/Test Scope Installer")]
    public sealed class TestScopeInstallerSO : ScopeInstallerSO
    {
        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {
            Debug.Log("TestScopeInstallerSO.Install");
        }
    }
}
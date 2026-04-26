using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    [CreateAssetMenu(
    fileName = "TestInstaller",
    menuName = "RPG/Core/Scene Loading/Test Installer")]
    public sealed class TestInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            Debug.Log("TestInstallerSO.Install");
        }
    }
}
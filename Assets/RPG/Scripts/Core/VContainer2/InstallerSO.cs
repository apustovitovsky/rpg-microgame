using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    public abstract class InstallerSO : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }

    public abstract class ScopeInstallerSO : ScriptableObject
    {
        public abstract void Install(LifetimeScope scope, IContainerBuilder builder);
    }
}
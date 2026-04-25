using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public abstract class ScopeInstallerSO : ScriptableObject
    {
        public abstract void Install(LifetimeScope scope, IContainerBuilder builder);
    }
}
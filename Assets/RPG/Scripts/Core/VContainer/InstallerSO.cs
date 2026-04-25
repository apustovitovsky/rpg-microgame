using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public abstract class InstallerSO : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
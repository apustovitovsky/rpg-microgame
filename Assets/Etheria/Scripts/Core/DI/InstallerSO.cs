using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public abstract class InstallerSO : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}

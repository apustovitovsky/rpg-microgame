using UnityEngine;
using VContainer;

namespace Etheria.Core.DI
{
    public abstract class ScopeInstallerSO : ScriptableObject
    {
        public abstract void Install(IContainerBuilder builder, GameObject rootObject);
    }
}

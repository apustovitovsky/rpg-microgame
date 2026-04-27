using UnityEngine;
using VContainer;

namespace Etheria.Core.DI
{
    public abstract class InstallerSO : ScriptableObject
    {
        public abstract void Install(IContainerBuilder builder, GameObject rootObject);
    }
}

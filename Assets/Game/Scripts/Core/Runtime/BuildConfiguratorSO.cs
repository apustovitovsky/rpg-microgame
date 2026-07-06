using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public abstract class BuildConfiguratorSO :
        ScriptableObject,
        IInstaller
    {
        public abstract void Install(IContainerBuilder builder);

        public virtual void Install(
            IContainerBuilder builder,
            Transform root)
        {
            Install(builder);
        }
    }
}
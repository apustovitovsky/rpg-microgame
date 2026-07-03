using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public abstract class BuildConfigurationSO :
        ScriptableObject,
        IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    public abstract class InstallerSO : ScriptableObject
    {
        public abstract void Install(IContainerBuilder builder, SceneScopeContext context);
    }
}
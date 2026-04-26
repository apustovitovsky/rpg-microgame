using UnityEngine;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    [CreateAssetMenu(
        menuName = "RPG/Core/Scene Loading/Scene Definition",
        fileName = "SceneDefinition")]
    public sealed class SceneDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string ScenePath { get; private set; }

        [field: SerializeField] public LifetimeScope LifetimeScope { get; private set; }

        [field: SerializeField] public InstallerSO[] ExtraInstallers { get; private set; }
    }
}
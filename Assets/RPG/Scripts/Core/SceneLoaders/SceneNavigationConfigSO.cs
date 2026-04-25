using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(
        fileName = "SceneNavigationConfig",
        menuName = "RPG/Core/Scene Loading/Scene Navigation Config")]
    public sealed class SceneNavigationConfigSO : ScriptableObject
    {
        [field: SerializeField] public ExperienceDefinitionSO StartupExperience { get; private set; }
        [field: SerializeField] public ExperienceDefinitionSO MainMenuExperience { get; private set; }
        [field: SerializeField] public ExperienceDefinitionSO RpgExperience { get; private set; }
        [field: SerializeField] public ExperienceDefinitionSO FpsExperience { get; private set; }
        [field: SerializeField] public ExperienceDefinitionSO SyntyExperience { get; private set; }
    }
}

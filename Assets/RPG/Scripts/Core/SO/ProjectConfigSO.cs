
using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ProjectConfig", menuName = "RPG/Core/Project Config")]
    public class ProjectConfigSO : ScriptableObject
    {
        [field: SerializeField] public LoadingScreenView LoadingScreenView { get; private set; }
        [field: SerializeField] public string MainMenuScene { get; private set; }
        [field: SerializeField] public string RPGScene { get; private set; }
        [field: SerializeField] public string FPSScene { get; private set; }
        [field: SerializeField] public string SyntyScene { get; private set; }
    }
}

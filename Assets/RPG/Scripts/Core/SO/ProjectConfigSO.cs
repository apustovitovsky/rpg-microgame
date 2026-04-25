
using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ProjectConfig", menuName = "RPG/Core/Project Config")]
    public class ProjectConfigSO : ScriptableObject
    {
        [field: SerializeField] public LoadingScreenView LoadingScreenView { get; private set; }
        [field: SerializeField] public SceneStackSO StartupSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO MainMenuSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO RPGSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO FPSSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO SyntySceneStack { get; private set; }
    }
}

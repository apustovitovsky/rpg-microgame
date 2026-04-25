using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(
        fileName = "SceneNavigationConfig",
        menuName = "RPG/Core/Scene Loading/Scene Navigation Config")]
    public sealed class SceneNavigationConfigSO : ScriptableObject
    {
        [field: SerializeField] public SceneStackSO StartupSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO MainMenuSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO RPGSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO FPSSceneStack { get; private set; }
        [field: SerializeField] public SceneStackSO SyntySceneStack { get; private set; }
    }
}

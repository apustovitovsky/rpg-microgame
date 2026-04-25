
using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ProjectConfig", menuName = "RPG/Core/Project Config")]
    public class ProjectConfigSO : ScriptableObject
    {
        [field: SerializeField] public LoadingScreenView LoadingScreenView { get; private set; }
    }
}

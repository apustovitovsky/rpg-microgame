using UnityEngine;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "ActorConfig", menuName = "Etheria/Gameplay/Actor/Actor Config")]
    public sealed class ActorConfigSO : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; } = "Actor";
    }
}


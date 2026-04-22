using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorConfig", menuName = "RPG/Gameplay/Actor/Actor Config")]
    public sealed class ActorConfigSO : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; } = "Actor";
        [field: SerializeField] public PickupDefinitionSO Pickup { get; private set; }
    }
}

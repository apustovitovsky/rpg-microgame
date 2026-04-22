using UnityEngine;

namespace RPG.Gameplay
{
    public abstract class PickupDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, Min(1)] public int InitialStackCount { get; private set; } = 1;

        public abstract bool ApplyTo(IPickupCollector collector);
    }
}

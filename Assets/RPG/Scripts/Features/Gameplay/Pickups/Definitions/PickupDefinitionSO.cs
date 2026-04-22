using UnityEngine;

namespace RPG.Gameplay
{
    public abstract class PickupDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        public abstract bool ApplyTo(IPickupCollector collector);
    }
}

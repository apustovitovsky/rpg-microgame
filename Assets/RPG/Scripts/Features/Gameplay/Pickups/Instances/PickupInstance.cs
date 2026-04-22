using UnityEngine;

namespace RPG.Gameplay
{
    public class PickupInstance : IPickupInstance
    {
        private readonly PickupDefinitionSO _definition;
        private readonly WorldPickup _worldPickup;

        public PickupInstance(PickupDefinitionSO definition, WorldPickup worldPickup)
        {
            _definition = definition;
            _worldPickup = worldPickup;
        }

        public string DisplayName => _definition != null ? _definition.DisplayName : string.Empty;
        public WorldPickup WorldPickup => _worldPickup;

        public bool TryCollect(IPickupCollector collector)
        {
            if (_definition == null)
                return false;

            if (!_definition.ApplyTo(collector))
                return false;

            OnCollected();
            return true;
        }

        protected virtual void OnCollected() { }
    }
}

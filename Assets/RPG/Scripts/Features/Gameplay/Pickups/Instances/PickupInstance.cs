using UnityEngine;

namespace RPG.Gameplay
{
    public class PickupInstance : IPickupInstance
    {
        private readonly PickupDefinitionSO _definition;

        public PickupInstance(PickupDefinitionSO definition)
        {
            _definition = definition;
        }

        public string DisplayName => _definition != null ? _definition.DisplayName : string.Empty;


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

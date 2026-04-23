using UnityEngine;
using System.Collections.Generic;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupDefinition", menuName = "RPG/Gameplay/Pickup/Pickup Definition")]
    public sealed class PickupDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [SerializeField] private PickupFragmentSO[] fragments;

        public T GetFragment<T>() where T : PickupFragmentSO
        {
            foreach (var fragment in fragments)
            {
                if (fragment is T typedFragment)
                    return typedFragment;
            }

            return null;
        }

        public IEnumerable<T> GetFragments<T>() where T : PickupFragmentSO
        {
            foreach (var fragment in fragments)
            {
                if (fragment is T typedFragment)
                    yield return typedFragment;
            }
        }
    }
}

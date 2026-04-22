using UnityEngine;

namespace RPG.Gameplay
{
    public abstract class PickupDefinitionSO : ScriptableObject
    {
        public abstract IPickupInstance CreateInstance();
    }
}

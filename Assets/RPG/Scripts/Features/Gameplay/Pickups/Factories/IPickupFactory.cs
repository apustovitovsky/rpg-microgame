using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPickupFactory
    {
        Pickup Create(PickupDefinitionSO definition, Vector3 position);
    }
}
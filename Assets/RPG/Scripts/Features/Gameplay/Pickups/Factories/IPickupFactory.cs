using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPickupFactory
    {
        void Create(Pickup prefab, Vector3 position);
    }
}
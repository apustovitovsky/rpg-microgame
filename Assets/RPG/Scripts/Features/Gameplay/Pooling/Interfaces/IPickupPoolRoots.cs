using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPickupPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
using UnityEngine;

namespace RPG.Gameplay
{
    public interface IActorPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
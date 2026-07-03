using UnityEngine;

namespace Game.Actor
{
    public interface IActorNameplatePoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
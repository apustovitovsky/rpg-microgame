using UnityEngine;

namespace Game.Actor
{
    public interface ITargetNameplatePoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
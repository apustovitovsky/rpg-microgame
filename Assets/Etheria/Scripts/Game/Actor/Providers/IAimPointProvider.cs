using UnityEngine;

namespace Etheria.Game.Actor
{
    public interface IAimPointProvider
    {
        Transform AimPoint { get; }
    }
}

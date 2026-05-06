using UnityEngine;

namespace Etheria.Game.Actor
{
    public interface ICameraPivotProvider
    {
        Transform CameraPivot { get; }
    }
}

using System;
using UnityEngine;

namespace Etheria.Game.Targeting
{
    public interface IPlayerTargetProvider
    {
        Transform CurrentTarget { get; }
        event Action<Transform> TargetChanged;
    }
}
using System;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Player
{
    public sealed class PlayerTargetService : IPlayerTargetService
    {
        public Transform CurrentTarget { get; private set; }

        public event Action<Transform> TargetChanged;

        public void SetTarget(Transform target)
        {
            if (CurrentTarget == target)
                return;

            CurrentTarget = target;
            TargetChanged?.Invoke(target);
        }

        public void Clear()
        {
            SetTarget(null);
        }
    }
}
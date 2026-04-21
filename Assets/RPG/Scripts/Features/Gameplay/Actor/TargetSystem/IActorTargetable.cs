using System;
using UnityEngine;

namespace RPG.Gameplay
{
    public interface IActorTargetable
    {
        public ActorId ActorId { get; }
        public string DisplayName { get; }
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}

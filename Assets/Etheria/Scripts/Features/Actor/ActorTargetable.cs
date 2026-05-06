using System;
using Etheria.Game.Actor;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public sealed class ActorTargetable : ITargetable
    {
        private readonly IPlayerAvatarInfo _info;
        private readonly Transform _root;

        public Guid Id => _info.Id;
        public Transform Root => _root;
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        public bool IsTargetable => true;


        public ActorTargetable(
            IPlayerAvatarInfo info,
            LifetimeScope scope,
            IAimPointProvider aimPointProvider,
            IUiAnchorProvider uiAnchorProvider)
        {
            _info = info;
            _root = scope.transform;
            AimPoint = aimPointProvider.AimPoint;
            UiAnchor = uiAnchorProvider.UiAnchor;
        }
    }
}


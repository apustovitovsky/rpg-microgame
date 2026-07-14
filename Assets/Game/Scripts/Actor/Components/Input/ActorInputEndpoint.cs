using System;
using Game.Core;
using Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorInputEndpoint :
        MonoBehaviour,
        IPrefabInstaller
    {
        [SerializeField] private ActorLookController _look;
        [SerializeField] private MovementController _movement;
        [SerializeField] private ActorTargetController _targeting;

        public void Install(
            IContainerBuilder builder)
        {
            if (_look == null ||
                _movement == null ||
                _targeting == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ActorInputEndpoint)} requires " +
                    $"{nameof(ActorLookController)}, " +
                    $"{nameof(MovementController)} and " +
                    $"{nameof(ActorTargetController)}.");
            }

            builder.RegisterComponent(_look);

            builder.RegisterComponent(_movement);

            builder.RegisterComponent(_targeting)
                .AsSelf()
                .As<ITargetProvider>();

            builder.Register<ActorInputBinder>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
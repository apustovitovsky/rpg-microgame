using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorRuntimeModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _focusPoint;

        public void Install(IContainerBuilder builder)
        {
            if (_root == null || _focusPoint == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ActorRuntimeModule)} requires assigned " +
                    $"{nameof(_root)} and {nameof(_focusPoint)}.");
            }

            builder.RegisterInstance(
                new ActorRuntimeAnchors(
                    _root,
                    _focusPoint));

            builder.RegisterEntryPoint<ActorRuntimeAnchorBinding>(
                Lifetime.Scoped);
        }
    }

    public readonly struct ActorRuntimeAnchors
    {
        public ActorRuntimeAnchors(
            Transform root,
            Transform focusPoint)
        {
            Root = root;
            FocusPoint = focusPoint;
        }

        public Transform Root { get; }

        public Transform FocusPoint { get; }
    }
}
using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavigationPatrolModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField]
        private NavigationPatrol _patrol;

        public void Install(
            IContainerBuilder builder)
        {
            if (_patrol == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(NavigationPatrolModule)} requires " +
                    $"a {nameof(NavigationPatrol)}.");
            }

            builder.RegisterComponent(_patrol)
                .AsSelf();
        }
    }
}
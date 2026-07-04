using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorScope : LifetimeScope
    {
        [Header("Extra Registrations")]
        [SerializeField] private ActorView _view;

        [Header("Build Configurations")]
        [SerializeField] private BuildConfigurationSO _core;
        [SerializeField] private BuildConfigurationSO _movement;
        [SerializeField] private BuildConfigurationSO _targeting;
        [SerializeField] private BuildConfigurationSO _combat;
        [SerializeField] private BuildConfigurationSO _ai;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_view == null)
            {
                Debug.LogError($"{nameof(ActorScope)} requires assigned {nameof(ActorView)}.", this);
                return;
            }

            builder.RegisterInstance(_view)
                .AsImplementedInterfaces();


            builder.Configure(_core);
            builder.Configure(_movement);
            builder.Configure(_targeting);
            builder.Configure(_combat);
            builder.Configure(_ai);
        }
    }
}
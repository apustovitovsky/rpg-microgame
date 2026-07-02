using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorLifetimeScope : LifetimeScope
    {
        [SerializeField] private ActorView _view;

        protected override void Configure(IContainerBuilder builder)
        {

            if (_view != null)
            {
                builder.RegisterComponent(_view)
                    .As<IActorView>();
            }
        }
    }
}
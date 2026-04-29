using System;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class ControlledTargetBinder : IStartable, IDisposable
    {
        private readonly IPlayerAvatarProvider _controlledActorProvider;
        private readonly IControlledTargetProvider _controlledTargetProvider;

        public ControlledTargetBinder(
            IPlayerAvatarProvider controlledActorProvider,
            IControlledTargetProvider controlledTargetProvider)
        {
            _controlledActorProvider = controlledActorProvider;
            _controlledTargetProvider = controlledTargetProvider;
        }

        public void Start()
        {
            _controlledActorProvider.Changed += OnControlledActorChanged;
            OnControlledActorChanged(_controlledActorProvider.Current);
        }

        public void Dispose()
        {
            _controlledActorProvider.Changed -= OnControlledActorChanged;
        }

        private void OnControlledActorChanged(PlayerAvatarContext? context)
        {
            if (context.HasValue)
            {
                _controlledTargetProvider.SetTarget(context.Value.Targetable);
                return;
            }

            _controlledTargetProvider.ClearTarget();
        }
    }
}

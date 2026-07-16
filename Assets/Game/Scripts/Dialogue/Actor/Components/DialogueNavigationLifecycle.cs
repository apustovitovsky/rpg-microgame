using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;

namespace Game.Dialogue.Actor
{
    public sealed class DialogueNavigationLifecycle :
        IDialogueParticipantLifecycle
    {
        private readonly IActorNavigation _navigation;

        public DialogueNavigationLifecycle(
            IActorNavigation navigation)
        {
            _navigation = navigation
                ?? throw new ArgumentNullException(nameof(navigation));
        }

        public UniTask<IUniTaskAsyncDisposable> EnterAsync(
            DialogueParticipantContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return UniTask.FromResult<IUniTaskAsyncDisposable>(
                new NavigationPauseLease(
                    _navigation.AcquirePause()));
        }

        private sealed class NavigationPauseLease :
            IUniTaskAsyncDisposable
        {
            private IDisposable _pause;

            public NavigationPauseLease(
                IDisposable pause)
            {
                _pause = pause
                    ?? throw new ArgumentNullException(nameof(pause));
            }

            public UniTask DisposeAsync()
            {
                var pause = _pause;
                _pause = null;

                pause?.Dispose();

                return UniTask.CompletedTask;
            }
        }
    }
}
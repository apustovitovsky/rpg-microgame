using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Yarn.Unity;

namespace Game.Dialogue.Yarn
{
    public sealed class YarnDialogueExecutor :
        IDialogueExecutor
    {
        private readonly DialogueRunner _runner;

        public YarnDialogueExecutor(
            DialogueRunner runner)
        {
            _runner = runner != null
                ? runner
                : throw new ArgumentNullException(nameof(runner));
        }

        public async UniTask ExecuteAsync(
            DialogueSession session,
            CancellationToken cancellationToken)
        {
            var project = _runner.YarnProject;

            if (project == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DialogueRunner)} requires a " +
                    $"{nameof(YarnProject)}.");
            }

            if (Array.IndexOf(
                    project.NodeNames,
                    session.Entry.NodeName) < 0)
            {
                throw new InvalidOperationException(
                    $"Yarn node '{session.Entry.NodeName}' " +
                    $"was not found in '{project.name}'.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            using var cancellationRegistration =
                cancellationToken.Register(Stop);

            await _runner.StartDialogue(
                session.Entry.NodeName);

            await _runner.DialogueTask;
        }

        public async UniTask StopAsync()
        {
            if (!_runner.IsDialogueRunning)
            {
                return;
            }

            await _runner.Stop();
        }

        private void Stop()
        {
            if (_runner.IsDialogueRunning)
            {
                _runner.Stop().Forget();
            }
        }
    }
}
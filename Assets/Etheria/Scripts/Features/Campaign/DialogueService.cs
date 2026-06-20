using System;
using Etheria.Game.Dialogue;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class DialogueService :
        IDialogueService,
        IDisposable
    {
        private readonly DialogueRunner _runner;
        private IDialogueParticipant _participant;

        public DialogueService(DialogueRunner runner)
        {
            _runner = runner;

            _runner.onDialogueComplete ??= new UnityEvent();
            _runner.onDialogueComplete.AddListener(OnDialogueCompleted);
        }

        public bool IsRunning => _runner.IsDialogueRunning;

        public bool TryStart(
            string nodeName,
            IDialogueParticipant participant,
            Transform interlocutor)
        {
            if (IsRunning || string.IsNullOrWhiteSpace(nodeName))
                return false;

            var project = _runner.YarnProject;

            if (project == null)
            {
                Debug.LogError("DialogueRunner has no YarnProject.");
                return false;
            }

            if (Array.IndexOf(project.NodeNames, nodeName) < 0)
            {
                Debug.LogError(
                    $"Yarn node '{nodeName}' was not found in '{project.name}'.");

                return false;
            }

            _participant = participant;
            _participant?.OnDialogueStarted(interlocutor);

            _runner.StartDialogue(nodeName).Forget();
            return true;
        }

        public void Dispose()
        {
            _runner.onDialogueComplete?.RemoveListener(OnDialogueCompleted);
        }

        private void OnDialogueCompleted()
        {
            _participant?.OnDialogueCompleted();
            _participant = null;
        }
    }
}
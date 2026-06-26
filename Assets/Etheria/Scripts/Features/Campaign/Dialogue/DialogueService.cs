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

        private readonly DialogueEntryCatalogSO _entryCatalog;

        public string DefaultSpeakerId { get; private set; }

        public DialogueService(
            DialogueRunner runner,
            DialogueEntryCatalogSO entryCatalog)
        {
            _runner = runner;
            _entryCatalog = entryCatalog;

            _runner.onDialogueComplete ??= new UnityEvent();
            _runner.onDialogueComplete.AddListener(OnDialogueCompleted);
        }

        private bool _isActive;

        public event Action Completed;

        public bool IsActive => _isActive;
        public bool IsRunning => _runner.IsDialogueRunning;


        public bool TryStart(
            string characterId,
            IDialogueParticipant participant,
            Transform interlocutor)
        {
            if (_isActive || string.IsNullOrWhiteSpace(characterId))
                return false;

            if (!_entryCatalog.TryGetNode(characterId, out var nodeName))
            {
                Debug.LogWarning(
                    $"No dialogue entry registered for character '{characterId}'.");

                return false;
            }

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

            DefaultSpeakerId = characterId;

            _participant = participant;
            _participant?.OnDialogueStarted(interlocutor);

            _isActive = true;
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
            DefaultSpeakerId = null;
            _isActive = false;

            Completed?.Invoke();
        }
    }
}
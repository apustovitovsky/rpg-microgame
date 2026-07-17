using System;
using UnityEngine;

namespace Game.Dialogue
{
    public readonly struct DialogueRequest
    {
        public DialogueRequest(
            Guid initiatorInstanceId,
            Guid speakerInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition,
            DialogueEntry entry)
        {
            InitiatorInstanceId = initiatorInstanceId;
            SpeakerInstanceId = speakerInstanceId;
            InitiatorPosition = initiatorPosition;
            SpeakerPosition = speakerPosition;
            Entry = entry;
        }

        public Guid InitiatorInstanceId { get; }

        public Guid SpeakerInstanceId { get; }

        public Vector3 InitiatorPosition { get; }

        public Vector3 SpeakerPosition { get; }

        public DialogueEntry Entry { get; }

        public bool IsValid =>
            InitiatorInstanceId != Guid.Empty &&
            SpeakerInstanceId != Guid.Empty &&
            InitiatorInstanceId != SpeakerInstanceId &&
            Entry.IsValid;
    }
}
using System;

namespace Game.Dialogue
{
    public readonly struct DialogueRequest
    {
        public DialogueRequest(
            Guid initiatorInstanceId,
            Guid speakerInstanceId,
            DialogueEntry entry)
        {
            InitiatorInstanceId = initiatorInstanceId;
            SpeakerInstanceId = speakerInstanceId;
            Entry = entry;
        }

        public Guid InitiatorInstanceId { get; }

        public Guid SpeakerInstanceId { get; }

        public DialogueEntry Entry { get; }

        public bool IsValid =>
            InitiatorInstanceId != Guid.Empty &&
            SpeakerInstanceId != Guid.Empty &&
            InitiatorInstanceId != SpeakerInstanceId &&
            Entry.IsValid;
    }
}
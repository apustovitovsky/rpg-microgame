using System;

namespace Game.Dialogue
{
    public sealed class DialogueSession
    {
        public DialogueSession(
            Guid initiatorInstanceId,
            Guid participantInstanceId)
        {
            if (initiatorInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue session initiator id is required.",
                    nameof(initiatorInstanceId));
            }

            if (participantInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue session participant id is required.",
                    nameof(participantInstanceId));
            }

            if (initiatorInstanceId == participantInstanceId)
            {
                throw new ArgumentException(
                    "Dialogue session participants must differ.",
                    nameof(participantInstanceId));
            }

            Id = Guid.NewGuid();
            InitiatorInstanceId = initiatorInstanceId;
            ParticipantInstanceId = participantInstanceId;
        }

        public Guid Id { get; }

        public Guid InitiatorInstanceId { get; }

        public Guid ParticipantInstanceId { get; }
    }
}
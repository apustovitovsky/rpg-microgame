using System;

namespace Game.Dialogue
{
    public sealed class DialogueSession
    {
        public DialogueSession(
            Guid id,
            DialogueRequest request)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue session id is required.",
                    nameof(id));
            }

            if (!request.IsValid)
            {
                throw new ArgumentException(
                    "Dialogue request is invalid.",
                    nameof(request));
            }

            Id = id;
            Request = request;
        }

        public Guid Id { get; }

        public DialogueRequest Request { get; }

        public Guid InitiatorInstanceId =>
            Request.InitiatorInstanceId;

        public Guid SpeakerInstanceId =>
            Request.SpeakerInstanceId;

        public DialogueEntry Entry =>
            Request.Entry;
    }
}
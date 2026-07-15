using System;

namespace Game.Dialogue
{
    public enum DialogueStartStatus
    {
        Started = 0,
        Busy = 1,
        InvalidRequest = 2
    }

    public readonly struct DialogueStartResult
    {
        public DialogueStartResult(
            DialogueStartStatus status,
            Guid sessionId)
        {
            Status = status;
            SessionId = sessionId;
        }

        public DialogueStartStatus Status { get; }

        public Guid SessionId { get; }

        public bool Succeeded =>
            Status == DialogueStartStatus.Started;

        public static DialogueStartResult Started(
            Guid sessionId)
        {
            return new DialogueStartResult(
                DialogueStartStatus.Started,
                sessionId);
        }

        public static DialogueStartResult Rejected(
            DialogueEvaluationStatus status)
        {
            var startStatus = status switch
            {
                DialogueEvaluationStatus.Busy =>
                    DialogueStartStatus.Busy,

                DialogueEvaluationStatus.InvalidRequest =>
                    DialogueStartStatus.InvalidRequest,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "An available dialogue cannot be rejected.")
            };

            return new DialogueStartResult(
                startStatus,
                Guid.Empty);
        }
    }
}
using System;

namespace Game.Dialogue
{
    public enum DialogueRunStatus
    {
        Completed = 0,
        Busy = 1,
        InvalidRequest = 2
    }

    public readonly struct DialogueRunResult
    {
        public DialogueRunResult(
            DialogueRunStatus status,
            Guid sessionId)
        {
            Status = status;
            SessionId = sessionId;
        }

        public DialogueRunStatus Status { get; }

        public Guid SessionId { get; }

        public bool Succeeded =>
            Status == DialogueRunStatus.Completed;

        public static DialogueRunResult Completed(
            Guid sessionId)
        {
            return new DialogueRunResult(
                DialogueRunStatus.Completed,
                sessionId);
        }

        public static DialogueRunResult Rejected(
            DialogueEvaluationStatus status)
        {
            var runStatus = status switch
            {
                DialogueEvaluationStatus.Busy =>
                    DialogueRunStatus.Busy,

                DialogueEvaluationStatus.InvalidRequest =>
                    DialogueRunStatus.InvalidRequest,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "An allowed dialogue cannot be rejected.")
            };

            return new DialogueRunResult(
                runStatus,
                Guid.Empty);
        }
    }
}
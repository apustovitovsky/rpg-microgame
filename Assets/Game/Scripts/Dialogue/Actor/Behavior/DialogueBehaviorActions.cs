using System;
using Game.AI;
using Game.Dialogue.Actor;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.Dialogue.Actor.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Face Dialogue Participant",
        story: "[Navigation] faces dialogue participant from [Participation]",
        category: "Game/Dialogue",
        id: "5cb7b2100ef34bb1a38160e5f3c4a8ac")]
    public partial class FaceDialogueParticipantAction :
        Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<DialogueParticipation>
            Participation;

        [SerializeReference]
        public BlackboardVariable<NavMeshNavigationModule>
            Navigation;

        protected override Status OnStart()
        {
            var participation = Participation?.Value;
            var navigation = Navigation?.Value;

            if (participation == null ||
                navigation == null ||
                !participation.TryGetContext(
                    out var context))
            {
                return Status.Failure;
            }

            Vector3 direction =
                context.OtherParticipantPosition -
                navigation.transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Status.Success;
            }

            navigation.FaceDirection(direction);

            return Status.Success;
        }
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Mark Dialogue Ready",
        story: "[Participation] is ready for dialogue",
        category: "Game/Dialogue",
        id: "7d3e7a799cd940b0af410c2137eeb929")]
    public partial class MarkDialogueReadyAction :
        Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<DialogueParticipation>
            Participation;

        protected override Status OnStart()
        {
            var participation = Participation?.Value;

            if (participation == null ||
                !participation.TryGetContext(
                    out var context))
            {
                return Status.Failure;
            }

            return participation.TryMarkReady(
                context.SessionId)
                ? Status.Success
                : Status.Failure;
        }
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Dialogue End",
        story: "wait until [Participation] leaves dialogue",
        category: "Game/Dialogue",
        id: "555d29c53ad24ef38a1ce2c0d2118c1b")]
    public partial class WaitForDialogueEndAction :
        Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<DialogueParticipation>
            Participation;

        [SerializeReference]
        public BlackboardVariable<NavMeshNavigationModule>
            Navigation;

        protected override Status OnStart()
        {
            return GetStatus();
        }

        protected override Status OnUpdate()
        {
            return GetStatus();
        }

        private Status GetStatus()
        {
            if (Participation?.Value != null &&
                Participation.Value.HasContext)
            {
                return Status.Running;
            }

            Navigation?.Value?.ClearFacing();

            return Status.Success;
        }
    }
}
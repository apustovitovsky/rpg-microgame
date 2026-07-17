using System;
using Unity.Behavior;
using Unity.Properties;

namespace Game.Dialogue.Actor.Behavior
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Is Dialogue Active",
        story: "[Participation] has an active dialogue",
        category: "Game/Dialogue",
        id: "f2852e390a2c42059727a246b8466e31")]
    public partial class IsDialogueActiveCondition :
        Condition
    {
        [UnityEngine.SerializeReference]
        public BlackboardVariable<DialogueParticipation>
            Participation;

        public override bool IsTrue()
        {
            return Participation?.Value != null &&
                   Participation.Value.HasContext;
        }
    }
}
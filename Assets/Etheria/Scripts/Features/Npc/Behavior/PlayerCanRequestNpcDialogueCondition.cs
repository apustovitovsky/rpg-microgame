using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Etheria.Npc.Behavior
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Player Can Request NPC Dialogue",
        story: "player can request dialogue with [Self]",
        category: "Etheria/Npc",
        id: "a1c7f8818e964a23a424d55f91273f10")]
    public partial class PlayerCanRequestNpcDialogueCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        public override bool IsTrue()
        {
            if (Self?.Value == null)
                return false;

            var sensor =
                Self.Value.GetComponentInParent<NpcAwarenessSensor>();

            return sensor != null &&
                   sensor.CanRequestDialogue;
        }
    }
}
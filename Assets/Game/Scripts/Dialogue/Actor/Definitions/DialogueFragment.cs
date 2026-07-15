using System;
using Game.Actor;
using UnityEngine;

namespace Game.Dialogue.Actor
{
    [Serializable]
    public sealed class DialogueFragment :
        ActorFragment
    {
        [field: SerializeField]
        public DialogueDefinition Definition { get; private set; }
    }
}
using System;
using UnityEngine;

namespace Etheria.Game.UI
{
    [Serializable]
    public sealed class CharacterLabelPoolRoots :
        ICharacterLabelPoolRoots
    {
        [field: SerializeField]
        public RectTransform ActiveRoot { get; private set; }

        [field: SerializeField]
        public RectTransform InactiveRoot { get; private set; }
    }
}
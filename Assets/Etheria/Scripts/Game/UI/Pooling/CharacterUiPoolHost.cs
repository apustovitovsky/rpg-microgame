using UnityEngine;

namespace Etheria.Game.UI
{
    public sealed class CharacterUiPoolHost : MonoBehaviour
    {
        [field: SerializeField]
        public CharacterLabelPoolRoots Labels { get; private set; }
    }
}
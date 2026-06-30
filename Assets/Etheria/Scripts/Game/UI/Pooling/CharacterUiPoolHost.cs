using UnityEngine;

namespace Etheria.Game.UI
{
    public sealed class CharacterUiPoolHost : MonoBehaviour
    {
        [field: SerializeField]
        public NpcNameLabelPoolRoots Labels { get; private set; }
    }
}
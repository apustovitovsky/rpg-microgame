using UnityEngine;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "WorldCharacterSetup",
        menuName = "Etheria/World/Character Setup")]
    public sealed class WorldCharacterSetupSO : ScriptableObject
    {
        [field: SerializeField]
        public WorldCharacterInitialState[] Characters
        {
            get;
            private set;
        }
    }
}
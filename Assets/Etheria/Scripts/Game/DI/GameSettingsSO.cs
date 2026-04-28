using Etheria.Core.DI;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Game.DI
{
    [CreateAssetMenu(
        fileName = "GameSettings",
        menuName = "Etheria/Game/Game Settings")]
    public sealed class GameSettingsSO : ScriptableObject
    {
        [field: SerializeField] public SceneCatalogSO SceneCatalog { get; private set; }
    }
}

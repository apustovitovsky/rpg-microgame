using Etheria.UI;
using UnityEngine;

namespace Etheria.Game.DI
{
    [CreateAssetMenu(
        fileName = "LoadingScreenSettings",
        menuName = "Etheria/Game/Loading Screen Settings")]
    public sealed class LoadingScreenSettingsSO : ScriptableObject
    {
        [field: SerializeField] public LoadingScreenView LoadingScreenView { get; private set; }
    }
}

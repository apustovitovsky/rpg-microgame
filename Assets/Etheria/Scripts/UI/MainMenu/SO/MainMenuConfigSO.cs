using UnityEditor;
using UnityEngine;


namespace Etheria.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuConfig", menuName = "Etheria/MainMenu/MainMenu Config")]
    public sealed class MainMenuConfigSO : ScriptableObject
    {
        [field: SerializeField] public MainMenuView MainMenuView { get; private set; }
    }
}
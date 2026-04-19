using UnityEditor;
using UnityEngine;


namespace RPG.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuConfig", menuName = "RPG/MainMenu/MainMenu Config")]
    public sealed class MainMenuConfigSO : ScriptableObject
    {
        [field: SerializeField] public MainMenuView MainMenuView { get; private set; }
    }
}
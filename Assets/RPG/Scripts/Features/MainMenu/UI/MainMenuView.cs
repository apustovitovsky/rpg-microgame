
using UnityEngine;

using UnityEngine.UI;

namespace RPG.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _beginPlayRPGButton;
        [SerializeField] private Button _beginPlayFPSButton;
        [SerializeField] private Button _exitGameButton;

        public Button BeginPlayRPGButton => _beginPlayRPGButton;
        public Button BeginPlayFPSButton => _beginPlayFPSButton;
        public Button ExitGameButton => _exitGameButton;
    }
}
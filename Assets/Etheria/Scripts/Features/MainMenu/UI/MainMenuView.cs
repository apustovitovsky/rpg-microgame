
using UnityEngine;

using UnityEngine.UI;

namespace Etheria.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _beginPlayRPGButton;
        [SerializeField] private Button _beginPlaySyntyButton;
        [SerializeField] private Button _beginPlayFPSButton;
        [SerializeField] private Button _exitGameButton;

        public Button BeginPlayRPGButton => _beginPlayRPGButton;
        public Button BeginPlaySyntyButton => _beginPlaySyntyButton;
        public Button BeginPlayFPSButton => _beginPlayFPSButton;
        public Button ExitGameButton => _exitGameButton;
    }
}
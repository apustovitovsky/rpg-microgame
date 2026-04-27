using Etheria.Core;
using VContainer.Unity;

namespace Etheria.MainMenu
{
    public class MainMenuPresenter : IStartable
    {
        private readonly MainMenuView _view;
        private readonly ISessionNavigator _sceneNavigator;

        public MainMenuPresenter(MainMenuView view, ISessionNavigator navigator)
        {
            _view = view;
            _sceneNavigator = navigator;
        }

        public void Start()
        {
            _view.BeginPlayRPGButton.onClick.AddListener(OnBeginPlayRPGClicked);
            _view.BeginPlayFPSButton.onClick.AddListener(OnBeginPlayFPSClicked);
            _view.ExitGameButton.onClick.AddListener(OnExitGameClicked);
            _view.BeginPlaySyntyButton.onClick.AddListener(OnBeginPlaySyntyClicked);
        }

        private void OnBeginPlayRPGClicked()
        {
            _sceneNavigator.LoadRPGScene();
        }

        private void OnBeginPlayFPSClicked()
        {
            _sceneNavigator.LoadFPSScene();
        }

        private void OnBeginPlaySyntyClicked()
        {
            _sceneNavigator.LoadSyntyScene();
        }

        private void OnExitGameClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

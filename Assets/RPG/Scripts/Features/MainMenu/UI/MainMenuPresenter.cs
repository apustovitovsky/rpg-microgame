using RPG.Core;
using UnityEngine;
using VContainer.Unity;

namespace RPG.MainMenu
{
    public class MainMenuPresenter : IStartable
    {
        private readonly MainMenuView _view;
        private readonly ISceneCoordinator _sceneCoordinator;

        public MainMenuPresenter(MainMenuView view, ISceneCoordinator coordinator)
        {
            _view = view;
            _sceneCoordinator = coordinator;
        }

        public void Start()
        {
            _view.BeginPlayRPGButton.onClick.AddListener(OnBeginPlayRPGClicked);
            _view.BeginPlayFPSButton.onClick.AddListener(OnBeginPlayFPSClicked);
            _view.ExitGameButton.onClick.AddListener(OnExitGameClicked);
        }

        private void OnBeginPlayRPGClicked()
        {
            _sceneCoordinator.LoadRPGScene();
        }

        private void OnBeginPlayFPSClicked()
        {
            _sceneCoordinator.LoadFPSScene();
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

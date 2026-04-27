using System;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace RPG.Game
{
    public sealed class LoadingScreenPresenter
    {
        private readonly LoadingScreenView _view;

        public LoadingScreenPresenter(LoadingScreenView view)
        {
            _view = view;
        }

        public void ShowLoadingScreen()
        {
            _view.SetLoadingScreenActive(true);
        }

        public void HideLoadingScreen()
        {
            _view.SetLoadingScreenActive(false);
        }
    }
}
using VContainer.Unity;
using Etheria.UI;

namespace Etheria.Game
{
    public interface ILoadingScreenService
    {
        public void ShowLoading();
        public void HideLoading();
    }

    public class LoadingScreenService : ILoadingScreenService, IStartable
    {
        private readonly LoadingScreenView _loadingScreenView;

        public LoadingScreenService(LoadingScreenView loadingScreenView) => _loadingScreenView = loadingScreenView;

        public void ShowLoading() => _loadingScreenView.SetLoadingScreenActive(true);
        public void HideLoading() => _loadingScreenView.SetLoadingScreenActive(false);

        public void Start()
        {
            HideLoading();
        }
    }
}
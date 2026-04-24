using VContainer.Unity;

namespace RPG.Core
{
    public interface IInstaller
    {
        void Install(in InstallContext context);
    }
}
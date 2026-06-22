using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Campaign
{
    [CreateAssetMenu(
        fileName = "CampaignUiInstaller",
        menuName = "Etheria/Features/Campaign/UI Installer")]
    public sealed class CampaignUiInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<QuestJournalView>();

            builder.RegisterEntryPoint<QuestJournalPresenter>(
                Lifetime.Singleton);
        }
    }
}
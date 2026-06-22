using Etheria.Core.DI;
using Etheria.Game.Quests;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Campaign
{
    [CreateAssetMenu(
        fileName = "CampaignStateInstaller",
        menuName = "Etheria/Features/Campaign/Campaign State Installer")]
    public sealed class CampaignStateInstallerSO : InstallerSO
    {
        [SerializeField] private QuestDefinitionSO[] _questDefinitions;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_questDefinitions);

            builder.Register<QuestService>(Lifetime.Singleton)
                .As<IQuestService>();

            builder.Register<QuestTextProvider>(Lifetime.Singleton)
                .As<IQuestTextProvider>();
        }
    }
}
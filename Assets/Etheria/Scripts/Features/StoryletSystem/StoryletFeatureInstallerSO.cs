using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Etheria.Features.StoryletSystem
{
    [CreateAssetMenu(
        fileName = "StoryletFeatureInstaller",
        menuName = "Etheria/Features/StoryletSystem/Storylet Feature Installer")]
    public class StoryletFeatureInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterEntryPoint<StoryletMatcherSmokeTestService>(Lifetime.Singleton);
            builder.Register<IEntityRoleFitEvaluator, AttributePreferenceEntityRoleFitEvaluator>(Lifetime.Singleton);
            builder.Register<IStoryletAssignmentBuilder, GreedyStoryletAssignmentBuilder>(Lifetime.Singleton);
            builder.Register<GreedyStoryletMatcher>(Lifetime.Singleton);
        }
    }
}


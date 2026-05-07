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

            builder.Register<AttributePreferenceEntityRoleFitEvaluator>(Lifetime.Singleton)
                .AsSelf()
                .As<IEntityRoleFitEvaluator>();

            builder.Register<GreedyStoryletAssignmentBuilder>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletAssignmentBuilder>();

            builder.Register<StoryletInstantiationService>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletInstantiationService>();

            builder.Register<StoryletEffectApplier>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletEffectApplier>();

            builder.Register<StoryletRepeatabilityService>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletRepeatabilityService>();

            builder.Register<StoryletSalienceEvaluator>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletSalienceEvaluator>();

            builder.Register<StoryletScoringService>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletScoringService>();

            builder.Register(resolver => new BeamStoryletPlanner(
                    resolver.Resolve<IStoryletInstantiationService>(),
                    resolver.Resolve<IStoryletEffectApplier>(),
                    resolver.Resolve<IStoryletRepeatabilityService>(),
                    resolver.Resolve<IStoryletSalienceEvaluator>(),
                    resolver.Resolve<IStoryletScoringService>(),
                    beamWidth: 3,
                    maxDepth: 6),
                Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletPlanner>();

            builder.Register<StoryletTelemetryFormatter>(Lifetime.Singleton)
                .AsSelf()
                .As<IStoryletTelemetryFormatter>();
        }
    }
}


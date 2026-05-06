using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Etheria.Features.NarrativeGeneration
{
    [CreateAssetMenu(
        fileName = "NarrativeGenerationFeatureInstaller",
        menuName = "Etheria/Features/NarrativeGeneration/Narrative Generation Feature Installer")]
    public class NarrativeGenerationFeatureInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<NarrativeGenerationService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<CompiledEventDefinitionFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<NarrativeGenerationEntryPoint>(Lifetime.Singleton);
        }
    }
}


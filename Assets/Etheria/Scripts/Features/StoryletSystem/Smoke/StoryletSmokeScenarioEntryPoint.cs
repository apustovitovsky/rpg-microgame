using System;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSmokeScenarioEntryPoint : IStartable
    {
        private readonly IStoryletSmokeScenarioProvider _scenarioProvider;
        private readonly IStoryletSimulationService _simulationService;
        private readonly IStoryletDiagnosticsFormatter _diagnosticsFormatter;

        public StoryletSmokeScenarioEntryPoint(
            IStoryletSmokeScenarioProvider scenarioProvider,
            IStoryletSimulationService simulationService,
            IStoryletDiagnosticsFormatter diagnosticsFormatter)
        {
            _scenarioProvider = scenarioProvider ?? throw new ArgumentNullException(nameof(scenarioProvider));
            _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
            _diagnosticsFormatter = diagnosticsFormatter ?? throw new ArgumentNullException(nameof(diagnosticsFormatter));
        }

        public void Start()
        {
            var request = _scenarioProvider.BuildRequest();
            var result = _simulationService.Simulate(request);
            Debug.Log(_diagnosticsFormatter.Format(result));
        }
    }
}

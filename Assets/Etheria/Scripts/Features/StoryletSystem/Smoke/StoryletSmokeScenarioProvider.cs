namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSmokeScenarioProvider : IStoryletSmokeScenarioProvider
    {
        public StoryletSimulationRequest BuildRequest()
        {
            var context = new StoryletSmokeCompilerContext(
                new OrderedSymbolRegistry<TagSymbol>("Tag", StoryletSmokeSymbols.Tags.All),
                new OrderedSymbolRegistry<AttributeSymbol>("Attribute", StoryletSmokeSymbols.Attributes.All),
                new OrderedSymbolRegistry<EntitySymbol>("Entity", StoryletSmokeSymbols.Entities.All),
                new OrderedSymbolRegistry<RoleSymbol>("Role", StoryletSmokeSymbols.Roles.All),
                new OrderedSymbolRegistry<StoryletSymbol>("Storylet", StoryletSmokeSymbols.Storylets.All));

            return new StoryletSimulationRequest(
                StoryletSmokeWorld.Build(context),
                StoryletSmokeStorylets.Build(context),
                StoryletPlannerMemory.Empty,
                includeDiagnostics: true,
                new StoryletPlannerOptions(beamWidth: 3, maxDepth: 6));
        }
    }
}

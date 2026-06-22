using System;
using Etheria.Game.World;
using VContainer.Unity;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class WorldFactCommandHandler :
        IStartable,
        IDisposable
    {
        private readonly DialogueRunner _runner;
        private readonly IWorldFactService _worldFacts;

        public WorldFactCommandHandler(
            DialogueRunner runner,
            IWorldFactService worldFacts)
        {
            _runner = runner;
            _worldFacts = worldFacts;
        }

        public void Start()
        {
            _runner.AddCommandHandler<string>(
                "set_world_fact",
                OnSetFact);

            _runner.AddCommandHandler<string>(
                "clear_world_fact",
                OnClearFact);

            _runner.AddFunction<string, bool>(
                "world_fact",
                IsFactSet);
        }

        public void Dispose()
        {
            _runner.RemoveCommandHandler("set_world_fact");
            _runner.RemoveCommandHandler("clear_world_fact");
            _runner.RemoveFunction("world_fact");
        }

        private void OnSetFact(string factId)
        {
            _worldFacts.TrySet(factId);
        }

        private void OnClearFact(string factId)
        {
            _worldFacts.TryClear(factId);
        }

        private bool IsFactSet(string factId)
        {
            return _worldFacts.IsSet(factId);
        }
    }
}
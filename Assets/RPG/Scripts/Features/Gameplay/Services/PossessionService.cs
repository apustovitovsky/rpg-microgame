namespace RPG.Gameplay
{
    public sealed class PossessionService : IPossessionService
    {
        private readonly IGameplayInputRouter _gameplayInput;
        private readonly ICameraService _cameraService;
        private IActorInputHandler _currentActorHandler;

        public PossessionService(
            IGameplayInputRouter gameplayInput,
            ICameraService cameraService)
        {
            _gameplayInput = gameplayInput;
            _cameraService = cameraService;
        }

        public void Possess(Actor actor)
        {
            Unpossess();

            _currentActorHandler = actor.InputHandler;

            _gameplayInput.SetHandler(_currentActorHandler);
            _cameraService.SetHandler(_currentActorHandler);
            _cameraService.SetTarget(actor.Root);
        }

        public void Unpossess()
        {
            if (_currentActorHandler == null) return;

            _gameplayInput.RemoveHandler(_currentActorHandler);
            _cameraService.RemoveHandler(_currentActorHandler);
            _cameraService.RemoveTarget();

            _currentActorHandler = null;
        }
    }
}

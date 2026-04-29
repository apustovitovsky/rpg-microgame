namespace Etheria.Game.Player
{
    public readonly struct PlayerAvatarHandlers
    {
        public PlayerAvatarHandlers(
            IActorInputHandler input,
            IActorFacingHandler facing)
        {
            Input = input;
            Facing = facing;
        }

        public IActorInputHandler Input { get; }
        public IActorFacingHandler Facing { get; }

        public bool IsValid =>
            Input != null &&
            Facing != null;
    }
}

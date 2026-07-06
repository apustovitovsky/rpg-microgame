using System;

namespace Game.Player
{
    public interface IPlayerInteractionInput
    {
        event Action InteractPerformed;
        event Action PossessPerformed;
    }
}
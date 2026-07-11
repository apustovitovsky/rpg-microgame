using System;

namespace Game.Player
{
    public interface IPlayerActionInput
    {
        event Action InteractPerformed;
        event Action PossessPerformed;
    }
}
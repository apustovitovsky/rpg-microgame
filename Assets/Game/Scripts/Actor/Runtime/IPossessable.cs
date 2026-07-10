using Game.Input;
using UnityEngine;

namespace Game.Actor
{
    public interface IPossessable
    {
        Transform CameraPivot { get; }

        void BindInput(IActorInput input);

        void UnbindInput();
    }
}
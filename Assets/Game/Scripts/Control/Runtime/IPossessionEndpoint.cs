using Game.Input;
using UnityEngine;

namespace Game.Control
{
    public interface IPossessionEndpoint
    {
        Transform Root { get; }

        Transform CameraPivot { get; }

        void BindInput(IControlInput input);

        void UnbindInput();
    }
}
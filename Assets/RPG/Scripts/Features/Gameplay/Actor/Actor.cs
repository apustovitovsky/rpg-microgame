using UnityEngine;

namespace RPG.Gameplay
{
    public readonly struct Actor
    {
        public readonly Transform Root;
        public readonly IActorInputHandler InputHandler;

        public Actor(Transform root, IActorInputHandler inputHandler)
        {
            Root = root;
            InputHandler = inputHandler;
        }
    }
}

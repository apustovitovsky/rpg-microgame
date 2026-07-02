using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    public interface IActorPerception
    {
        Transform Origin { get; }

        IReadOnlyCollection<GameObject> Candidates { get; }
    }
}
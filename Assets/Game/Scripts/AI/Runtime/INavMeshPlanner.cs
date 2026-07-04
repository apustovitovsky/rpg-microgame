using UnityEngine;

namespace Game.AI
{
    public interface INavMeshPlanner
    {
        bool HasDestination { get; }
        bool HasArrived { get; }
        bool IsNavigating { get; }
        Vector3 DesiredWorldDirection { get; }

        void MoveTo(Vector3 destination);
        void Stop();
    }
}
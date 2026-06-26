using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Game.Npc
{
    public interface INpcAgent
    {
        string NpcId { get; }

        // Legacy compatibility. Remove it after refactor.
        string CharacterId => NpcId;

        Transform Transform { get; }

        bool IsBusy { get; }

        bool TryMoveTo(
            Transform destination,
            Action<bool> completed = null);

        bool TryFollowRoute(
            IReadOnlyList<Transform> route,
            Action<bool> completed = null);

        void TeleportTo(
            Vector3 position,
            Quaternion rotation);

        void CancelAllTasks();
    }
}
using Game.World;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class WorldActor :
        MonoBehaviour,
        IWorldActor,
        IActorView,
        IDisplayable
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private Transform _uiAnchor;

        public WorldId WorldId { get; private set; }

        public ActorDefinition Definition { get; private set; }

        public Transform Root => transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public Transform CameraPivot => _cameraPivot != null
            ? _cameraPivot
            : Root;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : Root;

        public string DisplayName =>
            Definition != null && !string.IsNullOrWhiteSpace(Definition.DisplayName)
                ? Definition.DisplayName
                : WorldId.ToString();

        public void Initialize(
            WorldId worldId,
            ActorDefinition definition)
        {
            WorldId = worldId;
            Definition = definition;
        }
    }
}
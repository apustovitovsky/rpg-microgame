using Etheria.Core.Helpers;
using Etheria.Game.Actor;
using UnityEngine;

namespace Etheria.Features.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorRuntimeRefs : MonoBehaviour,
        IAimPointProvider,
        IUiAnchorProvider,
        ICameraPivotProvider
    {
        private const string DefaultAimPointName = "AimPoint";
        private const string DefaultCameraRootName = "PF_SyntyCamera";
        private const string DefaultCameraPivotName = "CameraPivot";
        private const string DefaultUiAnchorName = "UiAnchor";

        [field: SerializeField, ReadOnly] public Transform AimPoint { get; private set; }
        [field: SerializeField, ReadOnly] public Transform CameraRoot { get; private set; }
        [field: SerializeField, ReadOnly] public Transform CameraPivot { get; private set; }
        [field: SerializeField, ReadOnly] public Transform UiAnchor { get; private set; }
        [field: SerializeField, ReadOnly] public ActorHitbox[] Hitboxes { get; private set; }

        private void Reset()
        {
            AutoAssignIfMissing();
        }

        private void OnValidate()
        {
            AutoAssignIfMissing();

            Hitboxes = GetComponentsInChildren<ActorHitbox>(true);
        }

        private void AutoAssignIfMissing()
        {
            if (AimPoint == null)
                AimPoint = FindDeepChildByName(transform, DefaultAimPointName);

            if (CameraPivot == null)
                CameraPivot = FindDeepChildByName(transform, DefaultCameraPivotName);

            if (UiAnchor == null)
                UiAnchor = FindDeepChildByName(transform, DefaultUiAnchorName);

            if (CameraRoot == null)
                CameraRoot = FindDeepChildByName(transform, DefaultCameraRootName);
        }

        private static Transform FindDeepChildByName(Transform root, string childName)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == childName)
                    return child;
            }

            return null;
        }
    }
}


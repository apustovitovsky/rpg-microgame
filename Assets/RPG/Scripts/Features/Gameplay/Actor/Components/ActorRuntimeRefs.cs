using UnityEngine;

namespace RPG.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class ActorRuntimeRefs : MonoBehaviour
    {
        private const string DefaultAimPointName = "AimPoint";
        private const string DefaultCameraPivotName = "CameraPivot";
        private const string DefaultUiAnchorName = "UIAnchor";

        [field: SerializeField] public Transform AimPoint { get; private set; }
        [field: SerializeField] public Transform CameraPivot { get; private set; }
        [field: SerializeField] public Transform UiAnchor { get; private set; }
        [field: SerializeField] public ActorHitbox[] Hitboxes { get; private set; }

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

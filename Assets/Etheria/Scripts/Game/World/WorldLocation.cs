using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Etheria.Game.World
{
    public sealed class WorldLocation : MonoBehaviour
    {
        [SerializeField]
        private string _id;

        public string Id => _id;
        public Transform Transform => transform;

        private void OnValidate()
        {
            _id = _id?.Trim();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var groundPosition = transform.position;
            var labelPosition = groundPosition + Vector3.up * 1.6f;
            var lineEndPosition = labelPosition - Vector3.up * 0.2f;

            Gizmos.color = Color.white;

            Handles.color = Color.white;

            Handles.DrawWireDisc(
                groundPosition,
                Vector3.up,
                0.3f);
            Gizmos.DrawLine(groundPosition, lineEndPosition);
            Gizmos.DrawLine(
                groundPosition,
                groundPosition + transform.forward * 0.3f);

            var label = string.IsNullOrWhiteSpace(_id)
                ? name
                : _id;

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                }
            };

            Handles.Label(
                labelPosition,
                label,
                style);
        }
#endif
    }
}

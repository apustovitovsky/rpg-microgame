using TMPro;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcNameLabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;
        private Transform _anchor;
        private Camera _camera;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void Bind(
            Transform anchor,
            string displayName,
            Camera camera)
        {
            _anchor = anchor;
            _camera = camera;
            _nameText.text = displayName;

            _rectTransform ??= transform as RectTransform;

            UpdatePosition();
        }

        public void Clear()
        {
            _anchor = null;
            _camera = null;
            _nameText.text = string.Empty;
            SetVisible(false);
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_anchor == null || _camera == null)
            {
                SetVisible(false);
                return;
            }

            Vector3 screenPoint =
                _camera.WorldToScreenPoint(_anchor.position);

            bool isVisible = screenPoint.z > 0f;
            SetVisible(isVisible);

            if (isVisible)
                _rectTransform.position = screenPoint;
        }

        private void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}

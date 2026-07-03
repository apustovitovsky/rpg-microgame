using TMPro;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorNameplateView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;
        private Transform _anchor;
        private Camera _camera;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            SetVisible(false);
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        public void Bind(Transform anchor, string text, Camera camera)
        {
            _anchor = anchor;
            _camera = camera;

            if (_text != null)
                _text.text = text;

            _rectTransform ??= transform as RectTransform;
            UpdatePosition();
        }

        public void Clear()
        {
            _anchor = null;
            _camera = null;

            if (_text != null)
                _text.text = string.Empty;

            SetVisible(false);
        }

        private void UpdatePosition()
        {
            if (_anchor == null || _camera == null)
            {
                SetVisible(false);
                return;
            }

            Vector3 screenPoint = _camera.WorldToScreenPoint(_anchor.position);
            bool isVisible = screenPoint.z > 0f;

            SetVisible(isVisible);

            if (isVisible)
                _rectTransform.position = screenPoint;
        }

        private void SetVisible(bool visible)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
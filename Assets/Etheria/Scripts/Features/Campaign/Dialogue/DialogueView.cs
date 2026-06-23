using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Etheria.Features.Campaign
{
    public sealed class DialogueView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Line")]
        [SerializeField] private GameObject _speakerContainer;
        [SerializeField] private TMP_Text _speakerName;
        [SerializeField] private TMP_Text _lineText;

        [Header("Options")]
        [SerializeField] private Transform _optionsContainer;
        [SerializeField] private DialogueOptionView _optionPrefab;

        [Header("Panels")]
        [SerializeField] private GameObject _linePanel;
        [SerializeField] private GameObject _optionsPanel;

        private readonly List<DialogueOptionView> _options = new();

        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _linePanel.SetActive(false);
            _optionsPanel.SetActive(false);
        }

        public void Hide()
        {
            ClearOptions();

            _linePanel.SetActive(false);
            _optionsPanel.SetActive(false);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void SetLine(string speakerName, string text)
        {
            ClearOptions();

            _linePanel.SetActive(true);
            _optionsPanel.SetActive(false);

            bool hasSpeaker = !string.IsNullOrWhiteSpace(speakerName);

            _speakerContainer.SetActive(hasSpeaker);
            _speakerName.text = hasSpeaker ? speakerName : string.Empty;
            _lineText.text = text;
        }

        public void ShowOptions(
            IReadOnlyList<DialogueOptionViewModel> options)
        {
            ClearOptions();

            _linePanel.SetActive(false);
            _optionsPanel.SetActive(true);

            foreach (var option in options)
            {
                var view = Instantiate(
                    _optionPrefab,
                    _optionsContainer);

                view.Bind(option);
                _options.Add(view);
            }

            if (_options.Count > 0)
            {
                EventSystem.current?.SetSelectedGameObject(
                    _options[0].SelectionTarget);
            }
        }

        public void ClearOptions()
        {
            foreach (var option in _options)
            {
                option.Clear();
                Destroy(option.gameObject);
            }

            _options.Clear();
            _optionsPanel.SetActive(false);
        }
    }
}
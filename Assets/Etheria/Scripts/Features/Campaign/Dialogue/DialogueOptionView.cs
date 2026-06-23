using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Etheria.Features.Campaign
{
    public sealed class DialogueOptionView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        public GameObject SelectionTarget =>
    _button.gameObject;

        public void Bind(DialogueOptionViewModel model)
        {
            _text.text = model.Text;
            _button.interactable = model.IsAvailable;

            _button.onClick.RemoveAllListeners();

            if (model.IsAvailable)
                _button.onClick.AddListener(model.Selected.Invoke);
        }

        public void Clear()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
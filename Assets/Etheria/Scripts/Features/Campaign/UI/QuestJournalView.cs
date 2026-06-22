using TMPro;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    public sealed class QuestJournalView : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _journalText;

        public void Show(
            string title,
            string journal)
        {
            _titleText.text = title;
            _journalText.text = journal;

            _content.SetActive(true);
        }

        public void Hide()
        {
            _content.SetActive(false);
        }

        private void Awake()
        {
            Hide();
        }
    }
}
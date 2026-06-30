using Etheria.Game.Character;
using UnityEngine;
using VContainer;

namespace Etheria.Npc
{
    public sealed class NpcAwarenessSensor : MonoBehaviour
    {
        [SerializeField] private float _dialogueRange = 3f;

        private IPlayerCharacterProvider _playerCharacterProvider;
        private bool _dialogueRequestedInCurrentRange;

        public bool IsPlayerInDialogueRange
        {
            get
            {
                var player = _playerCharacterProvider?.Current;
                if (player == null)
                    return false;

                var delta = player.position - transform.position;
                delta.y = 0f;

                return delta.sqrMagnitude <= _dialogueRange * _dialogueRange;
            }
        }

        public bool CanRequestDialogue
        {
            get
            {
                var isPlayerNear = IsPlayerInDialogueRange;

                if (!isPlayerNear)
                {
                    _dialogueRequestedInCurrentRange = false;
                    return false;
                }

                return !_dialogueRequestedInCurrentRange;
            }
        }

        [Inject]
        public void Construct(
            IPlayerCharacterProvider playerCharacterProvider)
        {
            _playerCharacterProvider = playerCharacterProvider;
        }

        public void MarkDialogueRequested()
        {
            _dialogueRequestedInCurrentRange = true;
        }
    }
}
using UnityEngine;

namespace Game.Dialogue.Actor
{
    [CreateAssetMenu(
        fileName = "DialogueDefinition",
        menuName = "Game/Dialogue/Dialogue Definition")]
    public sealed class DialogueDefinition :
        ScriptableObject
    {
        [field: SerializeField]
        public string EntryNode { get; private set; }

        public DialogueEntry Entry =>
            new(EntryNode);

        private void OnValidate()
        {
            EntryNode = EntryNode?.Trim();
        }
    }
}
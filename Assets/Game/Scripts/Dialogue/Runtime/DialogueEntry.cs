namespace Game.Dialogue
{
    public readonly struct DialogueEntry
    {
        public DialogueEntry(string nodeName)
        {
            NodeName = nodeName?.Trim();
        }

        public string NodeName { get; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NodeName);
    }
}
using System;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    [CreateAssetMenu(
        fileName = "DialogueEntryCatalog",
        menuName = "Etheria/Features/Campaign/Dialogue Entry Catalog")]
    public sealed class DialogueEntryCatalogSO : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [field: SerializeField]
            public string CharacterId { get; private set; }

            [field: SerializeField]
            public string NodeName { get; private set; }
        }

        [SerializeField] private Entry[] _entries;

        public bool TryGetNode(
            string characterId,
            out string nodeName)
        {
            if (_entries != null)
            {
                foreach (var entry in _entries)
                {
                    if (entry != null &&
                        string.Equals(
                            entry.CharacterId,
                            characterId,
                            StringComparison.Ordinal))
                    {
                        nodeName = entry.NodeName;
                        return !string.IsNullOrWhiteSpace(nodeName);
                    }
                }
            }

            nodeName = null;
            return false;
        }
    }
}
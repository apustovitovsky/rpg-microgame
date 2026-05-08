using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [CreateAssetMenu(
        fileName = "TagCatalog",
        menuName = "Etheria/Features/StoryletSystem/Authoring/Tag Catalog")]
    public sealed class TagCatalogSO : ScriptableObject
    {
        [SerializeField]
        private string[] _tagIds;

        public IReadOnlyList<string> TagIds => _tagIds;

        public IEnumerable<string> GetEmptyOrWhitespaceIds()
        {
            return (_tagIds ?? System.Array.Empty<string>())
                .Where(string.IsNullOrWhiteSpace);
        }

        public IEnumerable<string> GetDuplicateIds()
        {
            return (_tagIds ?? System.Array.Empty<string>())
                .Where(tagId => !string.IsNullOrWhiteSpace(tagId))
                .GroupBy(tagId => tagId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
        }
    }
}

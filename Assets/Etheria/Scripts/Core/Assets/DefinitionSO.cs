using UnityEngine;

namespace Etheria.Core.Assets
{
    public abstract class DefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
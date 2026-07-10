using Game.Core;
using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorDefinition",
        menuName = "Game/Actor/Actor Definition")]
    public sealed class ActorDefinition : Definition
    {
        [SerializeField] private string _definitionId;

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        public string DefinitionId => _definitionId;

        protected override void OnValidate()
        {
            base.OnValidate();

            _definitionId = _definitionId?.Trim();
        }
    }
}
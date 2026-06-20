using System;
using Etheria.Core.Helpers;
using Etheria.Game.Actor;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class ActorIdentity : MonoBehaviour, IActorIdentity
    {
        [SerializeField, ReadOnlyField] private string _id;
        [SerializeField] private string _displayName = "";

        public Guid Id => Guid.TryParse(_id, out var id) ? id : Guid.Empty;
        public string DisplayName => _displayName;

        private void OnValidate()
        {
            if (!Guid.TryParse(_id, out _))
                _id = Guid.NewGuid().ToString();
        }

        [ContextMenu("Regenerate Id")]
        private void RegenerateId()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}
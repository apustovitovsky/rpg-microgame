using System;

namespace Game.Actor
{
    public sealed class ActorIdentity :
        IActorIdentity
    {
        private string _instanceId = string.Empty;
        private string _definitionId = string.Empty;

        public string InstanceId => _instanceId;
        public string DefinitionId => _definitionId;

        public void Initialize(
            string instanceId,
            string definitionId)
        {
            instanceId = instanceId?.Trim() ?? string.Empty;
            definitionId = definitionId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Actor instance id is required.", nameof(instanceId));

            if (string.IsNullOrWhiteSpace(definitionId))
                throw new ArgumentException("Actor definition id is required.", nameof(definitionId));

            _instanceId = instanceId;
            _definitionId = definitionId;
        }
    }
}
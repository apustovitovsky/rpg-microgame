using System;
using Game.Core;
using VContainer.Unity;

namespace Game.Inventory
{
    public sealed class InventoryOwnerRegistration :
        IInitializable,
        IDisposable
    {
        private readonly IInventoryOwner _owner;

        private readonly IRegistryWriter<IInventoryOwner>
            _owners;

        private bool _isRegistered;

        public InventoryOwnerRegistration(
            IInventoryOwner owner,
            IRegistryWriter<IInventoryOwner> owners)
        {
            _owner = owner
                ?? throw new ArgumentNullException(nameof(owner));

            _owners = owners
                ?? throw new ArgumentNullException(nameof(owners));
        }

        public void Initialize()
        {
            if (_isRegistered)
                return;

            _owners.Add(
                _owner.InstanceId,
                _owner);

            _isRegistered = true;
        }

        public void Dispose()
        {
            if (!_isRegistered)
                return;

            _owners.Remove(
                _owner.InstanceId,
                _owner);

            _isRegistered = false;
        }
    }
}
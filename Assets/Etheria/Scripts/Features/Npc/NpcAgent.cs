using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Etheria.Game.Interaction;
using Etheria.Game.Npc;
using UnityEngine;
using VContainer;

namespace Etheria.Npc
{
    public sealed class NpcAgent :
        MonoBehaviour,
        INpcIdentity,
        IInteractable
    {
        private INpcRuntimeRegistryWriter _runtimeRegistry;
        private INpcRuntime _runtime;
        private IActorCommandService _commands;
        private NpcDefinitionSO _definition;
        private bool _isRegistered;

        private Quaternion _homeRotation;

        public Quaternion HomeRotation =>
            _homeRotation;

        public string NpcId =>
            _definition != null
                ? _definition.NpcId
                : string.Empty;

        public bool CanInteract =>
            _runtime?.DialogueStarter?.CanStartDialogue == true;

        private void Awake()
        {
            _homeRotation = transform.rotation;
        }

        [Inject]
        public void Construct(
            INpcRuntimeRegistryWriter runtimeRegistry,
            INpcRuntime runtime,
            IActorCommandService commands,
            NpcDefinitionSO definition)
        {
            _runtimeRegistry = runtimeRegistry;
            _runtime = runtime;
            _commands = commands;
            _definition = definition;

            TryRegister();
        }

        public void Interact()
        {
            if (_commands == null ||
                string.IsNullOrWhiteSpace(NpcId))
            {
                return;
            }

            _commands.ExecuteAsync(
                    new StartDialogueCommand(
                        NpcId,
                        string.Empty),
                    this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            _runtime?.DialogueStarter?.Clear();
            Unregister();
        }

        private void TryRegister()
        {
            if (_isRegistered ||
                _runtimeRegistry == null ||
                _runtime == null ||
                !isActiveAndEnabled)
            {
                return;
            }

            _runtimeRegistry.Register(_runtime);
            _isRegistered = true;
        }

        private void Unregister()
        {
            if (!_isRegistered)
                return;

            _runtimeRegistry?.Unregister(_runtime);
            _isRegistered = false;
        }
    }
}
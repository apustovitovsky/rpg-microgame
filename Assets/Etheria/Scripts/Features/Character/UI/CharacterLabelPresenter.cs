using System;
using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterLabelPresenter :
        IStartable,
        IDisposable
    {
        private readonly ITargetProvider _targetProvider;
        private readonly CharacterLabelPool _pool;

        private readonly ICharacterNameProvider _characterNameProvider;

        private CharacterLabelView _currentView;
        private Camera _camera;

        public CharacterLabelPresenter(
            ITargetProvider targetProvider,
            CharacterLabelPool pool,
            ICharacterNameProvider characterNameProvider)
        {
            _targetProvider = targetProvider;
            _pool = pool;
            _characterNameProvider = characterNameProvider;
        }

        public void Start()
        {
            _camera = Camera.main;

            _targetProvider.TargetChanged += OnTargetChanged;
            OnTargetChanged(_targetProvider.CurrentTarget);
        }

        public void Dispose()
        {
            _targetProvider.TargetChanged -= OnTargetChanged;
            ReleaseCurrentView();
        }

        private void OnTargetChanged(ITargetCandidate target)
        {
            ReleaseCurrentView();

            if (target == null ||
                target.UiAnchor == null ||
                _camera == null)
            {
                return;
            }

            var characterId = GetCharacterId(target.Root);

            if (string.IsNullOrWhiteSpace(characterId))
                return;

            var displayName =
                _characterNameProvider.GetDisplayName(characterId);

            _currentView = _pool.Get(
                target.UiAnchor,
                displayName,
                _camera);
        }

        private static string GetCharacterId(
            Transform root)
        {
            if (root == null)
                return string.Empty;

            return root.GetComponentInParent(typeof(INpcAgent)) is INpcAgent npc
                ? npc.NpcId
                : string.Empty;
        }

        private void ReleaseCurrentView()
        {
            if (_currentView == null)
                return;

            _pool.Release(_currentView);
            _currentView = null;
        }
    }
}
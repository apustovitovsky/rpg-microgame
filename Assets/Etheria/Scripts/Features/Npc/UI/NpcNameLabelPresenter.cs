using System;
using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Npc
{
    public sealed class NpcNameLabelPresenter :
        IStartable,
        IDisposable
    {
        private readonly ITargetProvider _targetProvider;
        private readonly NpcNameLabelPool _pool;

        private readonly ICharacterNameProvider _characterNameProvider;

        private NpcNameLabelView _currentView;
        private Camera _camera;

        public NpcNameLabelPresenter(
            ITargetProvider targetProvider,
            NpcNameLabelPool pool,
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

            var npcId = GetNpcId(target.Root);

            if (string.IsNullOrWhiteSpace(npcId))
                return;

            var displayName =
                _characterNameProvider.GetDisplayName(npcId);

            _currentView = _pool.Get(
                target.UiAnchor,
                displayName,
                _camera);
        }

        private static string GetNpcId(
            Transform root)
        {
            if (root == null)
                return string.Empty;

            return root.GetComponentInParent(typeof(INpcIdentity)) is INpcIdentity npc
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
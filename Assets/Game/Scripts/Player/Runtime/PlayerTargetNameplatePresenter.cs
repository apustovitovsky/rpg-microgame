using Game.UI;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetNameplatePresenter :
        IStartable,
        System.IDisposable
    {
        private readonly IPlayerControl _control;
        private readonly TargetNameplatePool _pool;

        private TargetNameplateView _currentView;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerControl control,
            TargetNameplatePool pool)
        {
            _control = control;
            _pool = pool;
        }

        public void Start()
        {
            _control.ControlledObjectChanged +=
                RefreshCurrentTarget;

            _control.CurrentTargetChanged +=
                RefreshCurrentTarget;

            RefreshCurrentTarget();
        }

        public void Dispose()
        {
            _control.ControlledObjectChanged -=
                RefreshCurrentTarget;

            _control.CurrentTargetChanged -=
                RefreshCurrentTarget;

            ReleaseCurrent();
        }

        private void RefreshCurrentTarget()
        {
            var target = _control.CurrentTarget;

            if (target == null ||
                !target.IsTargetable ||
                target.InstanceId == System.Guid.Empty ||
                target.InstanceId ==
                _control.ControlledInstanceId ||
                string.IsNullOrWhiteSpace(target.DisplayName))
            {
                ReleaseCurrent();
                return;
            }

            var camera = ResolveCamera();

            if (camera == null)
            {
                ReleaseCurrent();
                return;
            }

            ReleaseCurrent();

            _currentView = _pool.Get(
                target.UiAnchor,
                target.DisplayName,
                camera);
        }

        private Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;

            _camera = Camera.main;
            return _camera;
        }

        private void ReleaseCurrent()
        {
            if (_currentView != null)
                _pool.Release(_currentView);

            _currentView = null;
        }
    }
}
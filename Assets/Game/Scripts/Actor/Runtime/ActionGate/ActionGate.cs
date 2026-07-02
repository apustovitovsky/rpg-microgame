using System;
using System.Collections.Generic;

namespace Game.Actor
{
    public sealed class ActionGate : IActionGate
    {
        private readonly Dictionary<string, ActorActionState> _states =
            new(StringComparer.Ordinal);

        public bool TryEnter(
            string actorId,
            ActorActionChannel channel,
            ActorActionChannel blocks,
            out IDisposable scope)
        {
            scope = null;

            if (string.IsNullOrWhiteSpace(actorId) ||
                channel == ActorActionChannel.None)
            {
                return false;
            }

            var state = GetOrCreate(actorId);

            if (state.IsBlocked(channel))
                return false;

            state.Enter(channel, blocks);
            scope = new Scope(this, actorId, channel, blocks);

            return true;
        }

        public bool IsBlocked(
            string actorId,
            ActorActionChannel channel)
        {
            if (string.IsNullOrWhiteSpace(actorId))
                return true;

            return _states.TryGetValue(actorId, out var state) &&
                   state.IsBlocked(channel);
        }

        private ActorActionState GetOrCreate(string actorId)
        {
            if (_states.TryGetValue(actorId, out var state))
                return state;

            state = new ActorActionState();
            _states[actorId] = state;
            return state;
        }

        private void Exit(
            string actorId,
            ActorActionChannel channel,
            ActorActionChannel blocks)
        {
            if (!_states.TryGetValue(actorId, out var state))
                return;

            state.Exit(channel, blocks);

            if (state.IsEmpty)
                _states.Remove(actorId);
        }

        private sealed class ActorActionState
        {
            private ActorActionChannel _active;
            private ActorActionChannel _blocked;

            public bool IsEmpty =>
                _active == ActorActionChannel.None &&
                _blocked == ActorActionChannel.None;

            public bool IsBlocked(ActorActionChannel channel)
            {
                return (_blocked & channel) != 0;
            }

            public void Enter(
                ActorActionChannel channel,
                ActorActionChannel blocks)
            {
                _active |= channel;
                _blocked |= blocks;
            }

            public void Exit(
                ActorActionChannel channel,
                ActorActionChannel blocks)
            {
                _active &= ~channel;
                _blocked &= ~blocks;
            }
        }

        private sealed class Scope : IDisposable
        {
            private readonly ActionGate _owner;
            private readonly string _actorId;
            private readonly ActorActionChannel _channel;
            private readonly ActorActionChannel _blocks;
            private bool _disposed;

            public Scope(
                ActionGate owner,
                string actorId,
                ActorActionChannel channel,
                ActorActionChannel blocks)
            {
                _owner = owner;
                _actorId = actorId;
                _channel = channel;
                _blocks = blocks;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _owner.Exit(_actorId, _channel, _blocks);
                _disposed = true;
            }
        }
    }
}
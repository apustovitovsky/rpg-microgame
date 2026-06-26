using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;

namespace Etheria.Npc
{
    public sealed class NpcTaskScheduler : IDisposable
    {
        private readonly List<INpcTask> _tasks = new();
        private INpcTask _currentTask;
        private CancellationTokenSource _currentCts;
        private bool _isRunning;

        public bool IsBusy =>
            _currentTask != null ||
            _tasks.Any(IsActive);

        public void Enqueue(
            INpcTask task)
        {
            if (task == null)
                return;

            if (HasBlockingTask())
                return;

            MergeOrAdd(task);

            TryInterruptCurrent();

            if (!_isRunning)
                RunLoopAsync().Forget();
        }

        private bool HasBlockingTask()
        {
            if (_currentTask != null &&
                _currentTask.IsBlocking)
            {
                return true;
            }

            return _tasks.Any(task =>
                task.IsBlocking &&
                IsActive(task));
        }

        public void CancelCurrentTask()
        {
            if (_currentTask == null ||
                _currentCts == null)
            {
                return;
            }

            _currentCts.Cancel();
        }

        public void CancelAll()
        {
            _tasks.Clear();
            CancelCurrentTask();
        }

        public void Dispose()
        {
            CancelAll();
            _currentCts?.Dispose();
        }

        private async UniTaskVoid RunLoopAsync()
        {
            _isRunning = true;

            try
            {
                while (true)
                {
                    var next = SelectNextTask();

                    if (next == null)
                        break;

                    _currentTask = next;
                    _currentCts = new CancellationTokenSource();

                    try
                    {
                        await _currentTask.RunAsync(
                            _currentCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch
                    {
                        // Task should set Failed itself if it handles exceptions.
                        throw;
                    }
                    finally
                    {
                        CleanupCurrent();
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private INpcTask SelectNextTask()
        {
            return _tasks
                .Where(IsActive)
                .OrderByDescending(task => task.Priority)
                .FirstOrDefault();
        }

        private void MergeOrAdd(
            INpcTask incoming)
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                var existing = _tasks[i];

                if (existing.Type != incoming.Type)
                    continue;

                if (ReferenceEquals(existing, _currentTask))
                {
                    _tasks[i] = incoming;

                    if (_currentTask.CanSuspend)
                        CancelCurrentTask();

                    return;
                }

                _tasks[i] = incoming;
                return;
            }

            _tasks.Add(incoming);
        }

        private void TryInterruptCurrent()
        {
            if (_currentTask == null ||
                !_currentTask.CanSuspend ||
                _currentCts == null)
            {
                return;
            }

            var next = SelectNextTask();

            if (next == null ||
                ReferenceEquals(next, _currentTask))
            {
                return;
            }

            if (next.Priority <= _currentTask.Priority)
                return;

            _currentCts.Cancel();
        }

        private void CleanupCurrent()
        {
            if (_currentTask != null &&
                IsFinished(_currentTask))
            {
                _tasks.Remove(_currentTask);
            }

            _currentCts?.Dispose();
            _currentCts = null;
            _currentTask = null;
        }

        private static bool IsActive(
            INpcTask task)
        {
            return task.Status == NpcTaskStatus.Pending ||
                   task.Status == NpcTaskStatus.Running ||
                   task.Status == NpcTaskStatus.Suspended;
        }

        private static bool IsFinished(
            INpcTask task)
        {
            return task.Status == NpcTaskStatus.Completed ||
                   task.Status == NpcTaskStatus.Failed ||
                   task.Status == NpcTaskStatus.Cancelled;
        }
    }
}
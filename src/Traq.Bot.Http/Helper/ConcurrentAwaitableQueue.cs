using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;

namespace Traq.Bot.Http.Helper
{
    class ConcurrentAwaitableQueue<T>
    {
        SpinLock _lock = new();
        readonly ConcurrentQueue<T> _queue = [];
        readonly ReusableValueTaskSource<T> _vts = new();
        bool _vtsInUse = false;

        public void Enqueue(T item)
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                if (_vtsInUse)
                {
                    _vtsInUse = false;
                    _vts.TryReset();
                    _vts.SetResult(item);
                }
                else
                {
                    // No one is waiting.
                    _queue.Enqueue(item);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }

        public ValueTask<T> DequeueAsync(CancellationToken ct)
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                if (_queue.TryDequeue(out var item))
                {
                    return ValueTask.FromResult(item);
                }
                else if (_vtsInUse)
                {
                    ThrowHelper.ThrowInvalidOperationException("Another DequeueAsync is already waiting.");
                }
                _vtsInUse = true;
                return _vts.WithCancellation(ct).AsValueTask();
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }
    }
}

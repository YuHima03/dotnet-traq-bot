using Microsoft.Extensions.ObjectPool;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace Traq.Bot.Http.Helpers
{
    class ReusableValueTaskSource<T> : IResettable, IValueTaskSource<T>
    {
        ManualResetValueTaskSourceCore<T> _core = new() { RunContinuationsAsynchronously = true };
        SpinLock _lock = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> AsValueTask() => new(this, _core.Version);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult(short token)
        {
            return _core.GetResult(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _core.GetStatus(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _core.OnCompleted(continuation, state, token, flags);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T result)
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                _core.SetResult(result);
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReset()
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                _core.Reset();
                return true;
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReusableValueTaskSource<T> WithCancellation(CancellationToken ct)
        {
            if (ct.CanBeCanceled)
            {
                var v = _core.Version;
                ct.Register(() =>
                {
                    if (_core.Version == v)
                    {
                        _core.SetException(new OperationCanceledException(ct));
                    }
                });
            }
            return this;
        }
    }
}

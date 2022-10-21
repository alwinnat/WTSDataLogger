using System.Collections.Concurrent;

namespace WTSDataLogger.Core
{
    public sealed class Spooler<T>
    {
        private BlockingCollection<T> _spool = new();

        #region Public API
        public Task? RunningTask { get; private set; }
        public int Count => _spool.Count;

        public void Add(T item)
        {
            _spool.Add(item);
        }
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void BeginStart(Action<T> onRunning)
        {
            if (onRunning == null)
                throw new ArgumentNullException(nameof(onRunning));

            RunningTask = Task.Run(() =>
            {
                foreach (var item in _spool.GetConsumingEnumerable())
                    onRunning(item);
            });
        }

        public void Stop() => _spool.CompleteAdding();
        #endregion
    }
}

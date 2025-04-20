using System.Collections.Concurrent;

namespace FileProcessorApp.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<QueuedFile> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(QueuedFile workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException(nameof(workItem));

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<QueuedFile> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem!;
        }
    }
}

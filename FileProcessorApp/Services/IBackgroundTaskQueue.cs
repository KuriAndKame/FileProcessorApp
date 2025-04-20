using FileProcessorApp.Services;

namespace FileProcessorApp.Services
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(QueuedFile workItem);
        Task<QueuedFile> DequeueAsync(CancellationToken cancellationToken);
    }
}

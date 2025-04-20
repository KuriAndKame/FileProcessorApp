using FileProcessorApp.Data;
using FileProcessorApp.Models;

namespace FileProcessorApp.Services
{
public class FileProcessingService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private FileSystemWatcher? _watcher;

   //private readonly string _watchFolder = Path.Combine(Directory.GetCurrentDirectory(), "files");
   private readonly string _watchFolder;
    public FileProcessingService(
        IBackgroundTaskQueue taskQueue,
        ILogger<FileProcessingService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _scopeFactory = scopeFactory;

        _watchFolder = config["FileProcessingSettings:WatchedDirectory"]!;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("Creating folder: {WatchFolder}", _watchFolder);
        Directory.CreateDirectory(_watchFolder); // на всякий случай, если папки нет

        _watcher = new FileSystemWatcher(_watchFolder, "*.txt")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += OnChanged;
        _watcher.Changed += OnChanged;
        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation("Started watching folder: {Folder}", _watchFolder);

        return ProcessQueueAsync(stoppingToken);
    }

    private void OnChanged(object? sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("File detected: {Event} -> {FilePath}", e.ChangeType, e.FullPath);

        var fileEntry = new FileEntry
        {
            //FileName = Path.GetFileName(e.FullPath)
            FileName = e.FullPath
        };

        _taskQueue.Enqueue(new QueuedFile
        {
            FileEntry = fileEntry,
            EventType = e.ChangeType.ToString()
        });
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var queuedItem = await ((BackgroundTaskQueue)_taskQueue).DequeueAsync(stoppingToken);
            var fileEntry = queuedItem.FileEntry;
            var eventType = queuedItem.EventType;

            var fullPath = Path.Combine(_watchFolder, fileEntry.FileName);
            if (!File.Exists(fullPath)) continue;

            _logger.LogInformation("Processing file: {FileName}, Event: {EventType}", fileEntry.FileName, eventType);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var content = await File.ReadAllTextAsync(fullPath, cancellationToken: stoppingToken);

            int wordCount = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            int lineCount = content.Split('\n').Length;
            int charCount = content.Length;

            // Ищем, есть ли уже запись по этому имени файла
            var existingEntry = db.Files.FirstOrDefault(x => x.FileName == fileEntry.FileName);
            if (existingEntry != null)
            {
                    fileEntry = existingEntry;
                    eventType = "Updated";
            }

                var statistics = new FileStatistics
            {
                Event = eventType,
                TimeStamp = DateTime.UtcNow,
                Words = wordCount,
                Lines = lineCount,
                Symbols = charCount,
                FileEntry = fileEntry
            };

            if (existingEntry == null)
            {
                db.Files.Add(fileEntry);
            }

            db.Statistics.Add(statistics);

            await db.SaveChangesAsync(stoppingToken);
        }
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        base.Dispose();
    }

    
}

}


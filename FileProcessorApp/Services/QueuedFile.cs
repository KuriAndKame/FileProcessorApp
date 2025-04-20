using FileProcessorApp.Models;

namespace FileProcessorApp.Services
{
    public class QueuedFile
    {
        public FileEntry FileEntry { get; set; } = null!;
        public string EventType { get; set; } = string.Empty;
    }
}

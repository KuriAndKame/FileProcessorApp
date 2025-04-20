using System.Text.Json.Serialization;

namespace FileProcessorApp.Models
{
    public class FileStatistics
    {
        /// <summary>
        /// Id - Id файла
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Event - статус файла(Created или Updated)
        /// </summary>
        public string Event { get; set; } = string.Empty;
        /// <summary>
        /// TimeStamp - Время создания файла
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Words - количество слов в файле
        /// </summary>
        public int Words { get; set; }
        /// <summary>
        /// Lines - количество строк в файле
        /// </summary>
        public int Lines { get; set; }
        /// <summary>
        /// Symbols - количество символов в файле
        /// </summary>
        public int Symbols { get; set; }

        /// <summary>
        /// Связь с FileEntry
        /// </summary>
        
        public int FileEntryId { get; set; }
        [JsonIgnore]
        public FileEntry FileEntry { get; set; } = null!;


    }
}

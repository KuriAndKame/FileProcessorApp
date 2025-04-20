namespace FileProcessorApp.Models
{
    public class FileEntry
    {
        /// <summary>
        /// Id - Id файла
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// FileName - Название файла
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>
        /// FullPath - путь к файлу
        /// </summary>
        //public string FullPath { get; set; } = string.Empty;
        /// <summary>
        /// Statistics - Статистические данные файла
        /// </summary>
        public List<FileStatistics> Statistics { get; set; } = new();
    }
}

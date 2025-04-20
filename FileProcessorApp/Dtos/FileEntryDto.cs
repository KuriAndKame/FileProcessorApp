using FileProcessorApp.Models;

namespace FileProcessorApp.Dtos
{
    public class FileEntryDto
    {
        /// <summary>
        /// Id - Id файла
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// FileName - Название файла
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// FullPath - путь к файлу
        /// </summary>
        //public string FullPath { get; set; } = string.Empty;
        /// <summary>
        /// Statistics - Статистические данные файла
        /// </summary>
        public List<FileStatisticsDto> Statistics { get; set; } = new();
    }
}

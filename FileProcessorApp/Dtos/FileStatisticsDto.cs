namespace FileProcessorApp.Dtos
{
    public class FileStatisticsDto
    {
        public int Id { get; set; }
        public string? Event { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Words { get; set; }
        public int Lines { get; set; }
        public int Symbols { get; set; }
    }
}

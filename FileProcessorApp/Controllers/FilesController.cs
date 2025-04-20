using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FileProcessorApp.Models; 
using Microsoft.EntityFrameworkCore;
using FileProcessorApp.Data;
using Microsoft.AspNetCore.Http.Metadata;
using FileProcessorApp.Dtos;
using FileProcessorApp.Services;


namespace FileProcessorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly string _uploadFolder;
        private readonly ILogger<FilesController> _logger;

        public FilesController(AppDbContext context, ILogger<FilesController> logger, IBackgroundTaskQueue taskQueue, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _taskQueue = taskQueue;
            _uploadFolder = config.GetValue<string>("FileProcessingSettings:WatchedDirectory") ?? throw new ArgumentNullException("FileProcessingSettings:WatchedDirectory");

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Choose a file.");

            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .txt files.");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("Max file's weight 10MB");

            var filePath = Path.Combine(_uploadFolder, file.FileName);
            


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File is uploaded: {FilePath}", filePath);

            var fileEntry = new FileEntry
            {
                FileName = Path.GetFileName(file.FileName),
                //FullPath = filePath
            };

            _taskQueue.Enqueue(new QueuedFile
            {
                FileEntry = fileEntry,
                EventType = "Created"
            });

            return Ok(new { file.FileName, status = "File is accepted and is being analyzed" });
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileEntryDto>>> GetFiles()
        {
            var files = await _context.Files
                .Include(f => f.Statistics).Select(f => new FileEntryDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    Statistics = f.Statistics.Select(s => new FileStatisticsDto
                    {
                        Id = s.Id,
                        Event = s.Event,
                        TimeStamp = s.TimeStamp,
                        Words = s.Words,
                        Lines = s.Lines,
                        Symbols = s.Symbols
                    }).ToList()
                }).ToListAsync();
            
          

            return Ok(files);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<FileEntryDto>> GetStatsById(int id)
        {
            var file = await _context.Files
                .Include(f => f.Statistics)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
                return NotFound("File not found.");

            var dto = new FileEntryDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Statistics = file.Statistics.Select(s => new FileStatisticsDto
                {
                    Id = s.Id,
                    Event = s.Event,
                    TimeStamp = s.TimeStamp,
                    Words = s.Words,
                    Lines = s.Lines,
                    Symbols = s.Symbols
                }).ToList()
            };

            return Ok(dto);
        }
    }
}

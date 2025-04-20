using FileProcessorApp.Data;
using FileProcessorApp.Services;
using FileProcessorApp.Settings;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace FileProcessorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;
            //Регистрация конфигурации
            //builder.Services.Configure<FileProcessingSettings>(builder.Configuration.GetSection("appsettings"));
            builder.Services.Configure<FileProcessingSettings>(builder.Configuration.GetSection("FileProcessingSettings"));
            string watchedDirectory = config["FileProcessingSettings:WatchedDirectory"]
                                      ?? throw new InvalidOperationException("WatchedDirectory not set");


            

            //Регистрация базы данных
            builder.Services.AddDbContext<AppDbContext>(op => op.UseInMemoryDatabase("FileDb"));

            //Регистрация Singleton 
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddHostedService<FileProcessingService>();
            //Регистрация Services

            // Add services to the container.

            builder.Services.AddControllers();

            //Ограничения
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 МБ
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "File API", Version = "v1" });
                c.OperationFilter<SwaggerFileOperationFilter>();
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            


            app.MapControllers();

            app.Run();
        }
    }
}

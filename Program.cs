using MySqlConnector;
using PlannerApp.FileStorage.Helpers;
using PlannerApp.FileStorage.Database;
using PlannerApp.FileStorage.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient(_ =>
    new MySqlConnection(DataHelper.GetStringDB(builder.Configuration))
    ); // Db Connection 
builder.Services.AddTransient<IDb,DatabaseProvider>();
builder.Services.AddScoped<IFileService,FileService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/base", ()=>{
    return AppDomain.CurrentDomain.BaseDirectory;
});
app.MapGet("/getfiles", () =>
{
    List<string> fileList = new List<string>();
    var volumeMountPath = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");

        if (!string.IsNullOrEmpty(volumeMountPath))
        {
            // Crea una ruta completa al directorio dentro del volumen
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,volumeMountPath);

            if (Directory.Exists(directoryPath))
            {
                // Utiliza Directory.GetFiles() o Directory.EnumerateFiles() para obtener la lista de archivos
                // Directory.GetFiles() devuelve una matriz de cadenas con los nombres de archivo completos
                // Directory.EnumerateFiles() devuelve un IEnumerable<string> para la iteración eficiente de archivos
                fileList = Directory.EnumerateFiles(directoryPath).ToList();
            }
        }
    return fileList;
});

app.MapPost("/files", async (HttpContext context,[FromServices] IFileService fileService) => {
    var protocol = "https://";
    var result = await fileService.UploadFile(context.Request.Form.Files[0], Int32.Parse(context.Request.Form["id"]), protocol+context.Request.Host.Host);
    return result;
});

app.MapGet("/files/{filename}", (string filename) =>{
      var filePath = DataHelper.GetVolumePath(builder.Configuration)+"/"+filename  ;                                       
        
       try{
            if(File.Exists(filePath))
            {
                
                var fileBytes = File.ReadAllBytes(filePath);

                
                var contentType = "image/" +Path.GetExtension(filename).Substring(1);
                

                
                return Results.File(filePath,contentType);
            }
            else{
                return Results.NotFound("Not exists  :" +filePath);
            }
       }catch(Exception e)
       {
            return Results.NotFound(e.Message);
       }

        // Leer el contenido del archivo
        
});

app.MapGet("/taskfiles/{id}", async(int id,[FromServices] IFileService fileService)=>{
    var result = await fileService.GetFilesById(id);
    return result;
});

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


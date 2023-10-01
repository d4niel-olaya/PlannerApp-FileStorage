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

app.MapGet("/weatherforecast", () =>
{
    return AppDomain.CurrentDomain.BaseDirectory.ToString();
})
.WithName("GetWeatherForecast");
app.MapPost("/files", async (HttpContext context,[FromServices] IFileService fileService) => {
    var protocol = "https://";
    if(context.Request.Host.Host == "localhost")
    {
        protocol = "http://";
    }
    var result = await fileService.UploadFile(context.Request.Form.Files[0], Int32.Parse(context.Request.Form["id"]), protocol+context.Request.Host.Host);
    return result;
});

app.MapGet("/files/{filename}", async (string filename) =>{
      var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"/files/filesTasks"); // Reemplaza con la ubicación real del archivo

       try{
            if(Directory.Exists(filePath))
            {
                
                var fileBytes = File.ReadAllBytes(Path.Combine(filePath, filename));

                Console.WriteLine(Path.Combine(filePath, filename));
                var contentType = "image/"+Path.GetExtension(Path.Combine(filePath)).ToString();
                Console.WriteLine(contentType);

                
                return Results.File(filePath,contentType);
            }
            else{
                return Results.NotFound();
            }
       }catch(Exception e)
       {
            return Results.NotFound();
       }

        // Leer el contenido del archivo
        
});

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}




using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using PlannerApp.FileStorage.Database;

namespace PlannerApp.FileStorage.Services;

public class FileService : IFileService
{
    private readonly IDb _dbService;

    public FileService(IDb db)
    {
        _dbService = db;
    }

    public async Task<ResponseHttp> GetFilesById(int id)
    {

        try
        {
            var list = new List<FileItem>();
            await _dbService.OpenDb();
            using var cmd = _dbService.GetCommand();
            cmd.Connection = _dbService.GetProvider();
            cmd.CommandText = "SELECT  Domain, FileName, FileUserName FROM Files WHERE TaskId = @F";
            cmd.Parameters.AddWithValue("@F", id);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var path = reader.GetString(0) + "/files/" + reader.GetString(1);
                var name = reader.GetString(2);
                var item = new FileItem
                {
                    Path = path,
                    Name = name
                };
                list.Add(item);
            }
            await _dbService.CloseDb();
            return new ResponseHttp(200, list);
        }
        catch (Exception e)
        {
            return new ResponseHttp(500, e.Message);
        }
    }

    public async Task<Response> UploadFile(IFormFile file, int taskId, string domain)
    {
        try
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var fileUserName = file.FileName;
            var volumeName = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_NAME");
            var volumeMountPath = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH")) ? "./files" : Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");

            if (!string.IsNullOrEmpty(volumeMountPath))
            {
                // Crea una ruta completa al directorio dentro del volumen
                var directoryPath = Path.Combine(volumeMountPath);

                // Asegúrate de que el directorio exista o créalo si no existe
                if (Directory.Exists(directoryPath) == false)
                {

                    Directory.CreateDirectory(directoryPath);
                }

                // Guarda el archivo dentro del directorio
                var filePath = Path.Combine(directoryPath, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.OpenReadStream().CopyToAsync(stream);

                }

                // Realiza aquí cualquier otra lógica necesaria, como guardar la ruta del archivo en tu base de datos.
            }

            await _dbService.OpenDb();
            using (var cmd = _dbService.GetCommand())
            {

                cmd.Connection = _dbService.GetProvider();
                cmd.CommandText = "INSERT INTO Files(TaskId, Domain, FileName, FileUserName) VALUE(@N, @D, @S, @F)";
                cmd.Parameters.AddWithValue("@N", taskId);
                cmd.Parameters.AddWithValue("@D", domain);
                cmd.Parameters.AddWithValue("@S", fileName);
                cmd.Parameters.AddWithValue("@F", fileUserName);
                await cmd.ExecuteNonQueryAsync();
            }
            await _dbService.CloseDb();

            return new Response(201, "Created");

        }
        catch (Exception e)
        {
            return new Response(500, e.Message);
        }
    }


}

public interface IFileService
{
    Task<Response> UploadFile([FromForm] IFormFile file, int taskId, string domain);
    //Task<List<File>> GetFiles();
    //IResult GetFile(string FileName);

    Task<ResponseHttp> GetFilesById(int id);
}

public class Response
{
    public int Code { get; set; }

    public string Message { get; set; }

    public Response(int code, string msg)
    {
        Code = code;
        Message = msg;
    }
}

public class ResponseHttp
{
    public int Code { get; set; }

    public object Object { get; set; }

    public ResponseHttp(int code, object obj)
    {
        Code = code;
        Object = obj;
    }
}

public class FileItem
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;


}

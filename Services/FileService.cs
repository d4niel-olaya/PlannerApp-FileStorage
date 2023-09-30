

using PlannerApp.FileStorage.Database;

namespace PlannerApp.FileStorage.Services;

public class FileService
{
    private readonly IDb _dbService;

    public FileService(IDb db)
    {
        _dbService = db;
    }
}
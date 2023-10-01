namespace PlannerApp.FileStorage.Helpers;
public class DataHelper
{
    public static string GetStringDB(IConfiguration configuration)
    {
        var devEnviroment = configuration["DbConnection:Db"];
        var productionEnviroment = Environment.GetEnvironmentVariable("DATABASE_URL");
        return string.IsNullOrEmpty(productionEnviroment) ? devEnviroment : productionEnviroment;
    }

    public static string GetVolumePath(IConfiguration configuration)
    {
        var devEnviroment = configuration["local:path"];
        var productionEnviroment = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
        return string.IsNullOrEmpty(productionEnviroment) ? devEnviroment : productionEnviroment;
    }
}

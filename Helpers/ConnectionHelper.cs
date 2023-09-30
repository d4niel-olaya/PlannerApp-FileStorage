namespace PlannerApp.FileStorage.Helpers;
public class DataHelper
{
    public static string GetStringDB(IConfiguration configuration)
    {
        var devEnviroment = configuration["DbConnection:Db"];
        var productionEnviroment = Environment.GetEnvironmentVariable("DATABASE_URL");
        return string.IsNullOrEmpty(productionEnviroment) ? devEnviroment : productionEnviroment;
    }
}
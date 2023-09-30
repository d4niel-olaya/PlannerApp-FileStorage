

using System.Data;
using System.Data.Common;
using MySqlConnector;

namespace PlannerApp.FileStorage.Database;


public class DatabaseProvider : IDb
{
    private readonly MySqlConnection _connection;


    public DatabaseProvider(MySqlConnection connection)
    {
        _connection = connection; 
    }


    public async Task OpenDb()
    {
        if(IsOpen() == false)
        {

            await _connection.OpenAsync();
        }
    }

    public async Task CloseDb()
    {
        await _connection.CloseAsync();
    }
    

    public MySqlCommand GetCommand()
    {
        return new MySqlCommand();
    }

    public MySqlConnection GetProvider()
    {
        return _connection;
    }
    public ConnectionState GetState()
    {
        return _connection.State;
    }
    public bool IsOpen()
    {
        if(GetState() == ConnectionState.Open)
        {
            return true;   
        }
        else{
            return false;
        }
    }
}


public interface IDb
{
    Task OpenDb();
    Task CloseDb();
    MySqlCommand GetCommand();

    MySqlConnection GetProvider();
    ConnectionState GetState();
    bool IsOpen();
}
using Microsoft.Data.Sqlite;

namespace Vega.Data.MasterLibrary.Database;

public static class MasterLibraryConnection
{
    private static readonly string DatabasePath =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "MasterLibrary.db");


    public static SqliteConnection Create()
    {
        var connection = new SqliteConnection(
            $"Data Source={DatabasePath}");

        connection.Open();

        return connection;
    }
}
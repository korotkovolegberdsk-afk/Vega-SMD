using Microsoft.Data.Sqlite;

namespace Vega.Data.MasterLibrary.Database;

public static class MasterLibraryMigrationRunner
{
    public static void Apply()
    {
        using var connection = MasterLibraryConnection.Create();

        CreateMigrationTable(connection);

        ExecuteMigration(
            connection,
            "001_MasterLibrary.sql");
    }


    private static void CreateMigrationTable(
        SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS MigrationHistory
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,

            FileName TEXT NOT NULL UNIQUE,

            AppliedAt TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }


    private static void ExecuteMigration(
        SqliteConnection connection,
        string fileName)
    {
        using var checkCommand = connection.CreateCommand();

        checkCommand.CommandText =
        """
        SELECT COUNT(*)
        FROM MigrationHistory
        WHERE FileName = $fileName;
        """;

        checkCommand.Parameters.AddWithValue(
            "$fileName",
            fileName);


        var exists =
            Convert.ToInt32(
                checkCommand.ExecuteScalar()) > 0;


        if (exists)
            return;


        var sql =
            File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Migration",
                    fileName));


        using var command = connection.CreateCommand();

        command.CommandText = sql;

        command.ExecuteNonQuery();


        using var historyCommand =
            connection.CreateCommand();

        historyCommand.CommandText =
        """
        INSERT INTO MigrationHistory
        (
            FileName,
            AppliedAt
        )
        VALUES
        (
            $fileName,
            $date
        );
        """;


        historyCommand.Parameters.AddWithValue(
            "$fileName",
            fileName);

        historyCommand.Parameters.AddWithValue(
            "$date",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


        historyCommand.ExecuteNonQuery();
    }
}
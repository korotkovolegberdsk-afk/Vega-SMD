using Microsoft.Data.Sqlite;

namespace Vega.Data.MasterLibrary.Database;

public static class MasterLibraryMigrationRunner
{
    public static void Apply()
    {
        using var connection = MasterLibraryConnection.Create();

        CreateMigrationTable(connection);

        ApplyMigrations(connection);
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


    private static void ApplyMigrations(
        SqliteConnection connection)
    {
        var migrationFolder =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Migration");


        Console.WriteLine(
            $"Migration folder: {migrationFolder}");


        if (!Directory.Exists(migrationFolder))
        {
            Console.WriteLine(
                "Migration folder not found");
            return;
        }


        var files =
            Directory.GetFiles(
                migrationFolder,
                "*.sql")
            .OrderBy(x => x)
            .ToList();


        Console.WriteLine("Found migrations:");

        foreach (var file in files)
        {
            Console.WriteLine(
                Path.GetFileName(file));
        }


        foreach (var file in files)
        {
            ExecuteMigration(
                connection,
                file);
        }
    }


    private static void ExecuteMigration(
        SqliteConnection connection,
        string filePath)
    {
        var fileName =
            Path.GetFileName(filePath);


        using var checkCommand =
            connection.CreateCommand();


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
        {
            Console.WriteLine(
                $"Skipped: {fileName}");

            return;
        }


        Console.WriteLine(
            $"Running SQL: {fileName}");


        var sql =
            File.ReadAllText(filePath);


        using var transaction =
            connection.BeginTransaction();


        try
        {
            using var command =
                connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = sql;

            command.ExecuteNonQuery();


            using var historyCommand =
                connection.CreateCommand();

            historyCommand.Transaction = transaction;


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
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss"));


            historyCommand.ExecuteNonQuery();


            transaction.Commit();


            Console.WriteLine(
                $"Completed: {fileName}");
        }
        catch(Exception ex)
        {
            transaction.Rollback();

            Console.WriteLine(
                $"Migration error: {fileName}");

            Console.WriteLine(
                ex.Message);

            throw;
        }
    }
}
using Microsoft.Data.Sqlite;

namespace Vega.Data.Migration;

public class DatabaseCleanup
{
    private readonly string _connectionString;


    public DatabaseCleanup()
    {
        _connectionString = "Data Source=SMT.db";
    }


    public void RemovePackageDuplicates()
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();

        command.CommandText =
        """
        DELETE FROM Packages
        WHERE Id NOT IN
        (
            SELECT MIN(Id)
            FROM Packages
            GROUP BY PackageName
        );
        """;


        command.ExecuteNonQuery();
    }
}
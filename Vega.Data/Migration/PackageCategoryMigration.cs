using Microsoft.Data.Sqlite;

namespace Vega.Data.Migration;

public class PackageCategoryMigration
{
    private readonly string _connectionString;


    public PackageCategoryMigration()
    {
        _connectionString = "Data Source=SMT.db";
    }


    public void FillDefaultPackageCategories()
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        SetCategoryAndFamily(connection, "0402", "CHIP", "0402");
        SetCategoryAndFamily(connection, "0603", "CHIP", "0603");
        SetCategoryAndFamily(connection, "0805", "CHIP", "0805");
        SetCategoryAndFamily(connection, "1206", "CHIP", "1206");

        SetCategoryAndFamily(connection, "SO8", "IC", "SO8");
        SetCategoryAndFamily(connection, "QFN32", "IC", "QFN32");
    }


    private static void SetCategoryAndFamily(
        SqliteConnection connection,
        string packageName,
        string category,
        string family)
    {
        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE Packages
        SET
            Category = $Category,
            Family = $Family
        WHERE PackageName = $PackageName;
        """;

        command.Parameters.AddWithValue("$PackageName", packageName);
        command.Parameters.AddWithValue("$Category", category);
        command.Parameters.AddWithValue("$Family", family);

        command.ExecuteNonQuery();
    }
}

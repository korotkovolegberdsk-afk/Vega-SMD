using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class MasterLibraryMigrationTests
{
    [Fact]
    public void Migration_Should_Create_PackageCategory_Table()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT name
        FROM sqlite_master
        WHERE type='table'
        AND name='PackageCategory';
        """;

        var result = command.ExecuteScalar();

        Assert.NotNull(result);
        Assert.Equal("PackageCategory", result!.ToString());
    }


    [Fact]
    public void Migration_Should_Create_PackageDefinition_Table()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT name
        FROM sqlite_master
        WHERE type='table'
        AND name='PackageDefinition';
        """;

        var result = command.ExecuteScalar();

        Assert.NotNull(result);
        Assert.Equal("PackageDefinition", result!.ToString());
    }


    [Fact]
    public void Migration_Should_Create_ComponentDefinition_Table()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT name
        FROM sqlite_master
        WHERE type='table'
        AND name='ComponentDefinition';
        """;

        var result = command.ExecuteScalar();

        Assert.NotNull(result);
        Assert.Equal("ComponentDefinition", result!.ToString());
    }


    [Fact]
    public void MigrationHistory_Should_Contain_003_Migration()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT FileName
        FROM MigrationHistory
        ORDER BY Id;
        """;

        using var reader = command.ExecuteReader();

        var migrations = new List<string>();

        while (reader.Read())
        {
            migrations.Add(reader.GetString(0));
        }


        Assert.Contains(
            "001_MasterLibrary.sql",
            migrations);


        Assert.Contains(
            "002_MasterLibrary_Packages.sql",
            migrations);


        Assert.Contains(
            "003_MasterLibrary_Components.sql",
            migrations);
    }
}
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class MasterLibraryMigrationTests
{
    [Fact]
    public void Migration_Should_Create_PackageCategory_Table()
    {
        // Запуск миграции
        MasterLibraryMigrationRunner.Apply();


        // Проверяем подключение
        using var connection =
            MasterLibraryConnection.Create();


        using var command =
            connection.CreateCommand();


        command.CommandText =
        """
        SELECT name
        FROM sqlite_master
        WHERE type='table'
        AND name='PackageCategory';
        """;


        var result =
            command.ExecuteScalar();


        Assert.NotNull(result);

        Assert.Equal(
            "PackageCategory",
            result.ToString());
    }
}
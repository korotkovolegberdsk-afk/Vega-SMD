using Vega.Data.MasterLibrary.Database;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class ComponentDefinitionRepositoryTests
{
    private static int PrepareDatabase()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection =
            MasterLibraryConnection.Create();

        using var command =
            connection.CreateCommand();

        command.CommandText =
        """
        INSERT OR IGNORE INTO PackageCategory
        (
            Code,
            Name
        )
        VALUES
        (
            'COMPONENT_REPOSITORY_TEST_CATEGORY',
            'Component Repository Test Category'
        );


        INSERT INTO PackageFamily
        (
            CategoryId,
            Code,
            Name
        )
        SELECT
            Id,
            'COMPONENT_REPOSITORY_TEST_FAMILY',
            'Component Repository Test Family'
        FROM PackageCategory
        WHERE Code = 'COMPONENT_REPOSITORY_TEST_CATEGORY'
          AND NOT EXISTS
          (
              SELECT 1
              FROM PackageFamily
              WHERE Code = 'COMPONENT_REPOSITORY_TEST_FAMILY'
          );


        INSERT OR IGNORE INTO PackageDefinition
        (
            PackageName,
            DisplayName,
            CategoryId,
            FamilyId
        )
        SELECT
            'COMPONENT_REPOSITORY_TEST_PACKAGE',
            'Component Repository Test Package',
            category.Id,
            family.Id
        FROM PackageCategory AS category
        INNER JOIN PackageFamily AS family
            ON family.CategoryId = category.Id
        WHERE category.Code = 'COMPONENT_REPOSITORY_TEST_CATEGORY'
          AND family.Code = 'COMPONENT_REPOSITORY_TEST_FAMILY';


        DELETE FROM ComponentDefinition
        WHERE ManufacturerPartNumber IN
        (
            'TEST-001',
            'TEST-002',
            'TEST-DELETE'
        );
        """;

        command.ExecuteNonQuery();


        command.CommandText =
        """
        SELECT Id
        FROM PackageDefinition
        WHERE PackageName = 'COMPONENT_REPOSITORY_TEST_PACKAGE'
        LIMIT 1;
        """;

        var packageId =
            command.ExecuteScalar();

        Assert.NotNull(packageId);

        return Convert.ToInt32(packageId);
    }


    [Fact]
    public void Add_And_GetAll_ComponentDefinition_Should_Work()
    {
        var packageId =
            PrepareDatabase();

        var repository =
            new ComponentDefinitionRepository();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "TEST-001",
            Description = "Test component",
            PackageId = packageId,
            ComponentType = "IC",
            Manufacturer = "TEST",
            Version = 1
        };


        repository.Add(component);


        var items =
            repository.GetAll();


        Assert.Contains(
            items,
            x => x.ManufacturerPartNumber == "TEST-001");
    }


    [Fact]
    public void GetById_Should_Return_ComponentDefinition()
    {
        var packageId =
            PrepareDatabase();

        var repository =
            new ComponentDefinitionRepository();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "TEST-002",
            Description = "GetById test",
            PackageId = packageId,
            ComponentType = "IC",
            Manufacturer = "TEST",
            Version = 1
        };


        repository.Add(component);


        var item =
            repository.GetAll()
            .First(x =>
                x.ManufacturerPartNumber == "TEST-002");


        var result =
            repository.GetById(item.Id);


        Assert.NotNull(result);

        Assert.Equal(
            item.ManufacturerPartNumber,
            result!.ManufacturerPartNumber);
    }


    [Fact]
    public void Delete_Should_Remove_ComponentDefinition()
    {
        var packageId =
            PrepareDatabase();

        var repository =
            new ComponentDefinitionRepository();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "TEST-DELETE",
            Description = "Delete test",
            PackageId = packageId,
            ComponentType = "IC",
            Manufacturer = "TEST",
            Version = 1
        };


        repository.Add(component);


        var item =
            repository.GetAll()
            .First(x =>
                x.ManufacturerPartNumber == "TEST-DELETE");


        repository.Delete(item.Id);


        var deleted =
            repository.GetById(item.Id);


        Assert.Null(deleted);
    }
}
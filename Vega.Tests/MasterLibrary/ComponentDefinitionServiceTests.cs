using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class ComponentDefinitionServiceTests
{
    private static int PrepareDatabase()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection =
            Vega.Data.MasterLibrary.Database.MasterLibraryConnection.Create();

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
            'SERVICE_TEST_CATEGORY',
            'Service Test Category'
        );


        INSERT INTO PackageFamily
        (
            CategoryId,
            Code,
            Name
        )
        SELECT
            Id,
            'SERVICE_TEST_FAMILY',
            'Service Test Family'
        FROM PackageCategory
        WHERE Code = 'SERVICE_TEST_CATEGORY'
          AND NOT EXISTS
          (
              SELECT 1
              FROM PackageFamily
              WHERE Code = 'SERVICE_TEST_FAMILY'
          );


        INSERT OR IGNORE INTO PackageDefinition
        (
            PackageName,
            DisplayName,
            CategoryId,
            FamilyId
        )
        SELECT
            'SERVICE_TEST_PACKAGE',
            'Service Test Package',
            category.Id,
            family.Id
        FROM PackageCategory category
        INNER JOIN PackageFamily family
            ON family.CategoryId = category.Id
        WHERE category.Code = 'SERVICE_TEST_CATEGORY'
          AND family.Code = 'SERVICE_TEST_FAMILY';


        DELETE FROM ComponentDefinition
        WHERE ManufacturerPartNumber IN
        (
            'SERVICE-001'
        );
        """;

        command.ExecuteNonQuery();


        command.CommandText =
        """
        SELECT Id
        FROM PackageDefinition
        WHERE PackageName = 'SERVICE_TEST_PACKAGE'
        LIMIT 1;
        """;


        return Convert.ToInt32(
            command.ExecuteScalar());
    }


    [Fact]
    public void Add_And_GetAll_Should_Work()
    {
        var packageId =
            PrepareDatabase();


        var service =
            new ComponentDefinitionService();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "SERVICE-001",
            Description = "Service test",
            PackageId = packageId,
            ComponentType = "IC",
            Manufacturer = "TEST",
            Version = 1
        };


        service.Add(component);


        var result =
            service.GetAll();


        Assert.Contains(
            result,
            x => x.ManufacturerPartNumber == "SERVICE-001");
    }


    [Fact]
    public void Add_Without_PartNumber_Should_Fail()
    {
        var service =
            new ComponentDefinitionService();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "",
            PackageId = 1
        };


        Assert.Throws<ArgumentException>(
            () => service.Add(component));
    }


    [Fact]
    public void Delete_Should_Work()
    {
        var packageId =
            PrepareDatabase();


        var service =
            new ComponentDefinitionService();


        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "SERVICE-001",
            Description = "Delete service test",
            PackageId = packageId,
            ComponentType = "IC",
            Manufacturer = "TEST",
            Version = 1
        };


        service.Add(component);


        var item =
            service.GetAll()
            .First(x =>
                x.ManufacturerPartNumber == "SERVICE-001");


        service.Delete(item.Id);


        var deleted =
            service.GetById(item.Id);


        Assert.Null(deleted);
    }
}
using Microsoft.Data.Sqlite;

namespace Vega.Data.MasterLibrary.Database;

public class MasterLibrarySeeder
{
    public static void Seed()
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
        ('CHIP','Chip Components'),
        ('IC','Integrated Circuits');


        INSERT OR IGNORE INTO PackageFamily
        (
            CategoryId,
            Code,
            Name
        )
        SELECT
            Id,
            'PASSIVE',
            'Passive Components'
        FROM PackageCategory
        WHERE Code='CHIP';


        INSERT OR IGNORE INTO PackageFamily
        (
            CategoryId,
            Code,
            Name
        )
        SELECT
            Id,
            'SOP',
            'Small Outline Package'
        FROM PackageCategory
        WHERE Code='IC';



        INSERT OR IGNORE INTO PackageDefinition
        (
            PackageName,
            DisplayName,
            CategoryId,
            FamilyId
        )
        SELECT
            '0603',
            '0603 Chip Resistor',
            c.Id,
            f.Id
        FROM PackageCategory c
        JOIN PackageFamily f
            ON f.CategoryId=c.Id
        WHERE c.Code='CHIP'
          AND f.Code='PASSIVE';



        INSERT OR IGNORE INTO PackageDefinition
        (
            PackageName,
            DisplayName,
            CategoryId,
            FamilyId
        )
        SELECT
            'SOP8',
            'SOP 8 Pin',
            c.Id,
            f.Id
        FROM PackageCategory c
        JOIN PackageFamily f
            ON f.CategoryId=c.Id
        WHERE c.Code='IC'
          AND f.Code='SOP';



        INSERT OR IGNORE INTO ComponentDefinition
        (
            ManufacturerPartNumber,
            Description,
            PackageId,
            ComponentType,
            Manufacturer,
            Version,
            CreatedAt,
            UpdatedAt
        )
        SELECT
            'R-0603-10K',
            'Chip resistor 10k',
            Id,
            'RESISTOR',
            'TEST',
            1,
            datetime('now'),
            datetime('now')
        FROM PackageDefinition
        WHERE PackageName='0603';



        INSERT OR IGNORE INTO ComponentDefinition
        (
            ManufacturerPartNumber,
            Description,
            PackageId,
            ComponentType,
            Manufacturer,
            Version,
            CreatedAt,
            UpdatedAt
        )
        SELECT
            'C-0603-100N',
            'Chip capacitor 100nF',
            Id,
            'CAPACITOR',
            'TEST',
            1,
            datetime('now'),
            datetime('now')
        FROM PackageDefinition
        WHERE PackageName='0603';



        INSERT OR IGNORE INTO ComponentDefinition
        (
            ManufacturerPartNumber,
            Description,
            PackageId,
            ComponentType,
            Manufacturer,
            Version,
            CreatedAt,
            UpdatedAt
        )
        SELECT
            'IC-SOP8-TEST',
            'Test IC SOP8',
            Id,
            'IC',
            'TEST',
            1,
            datetime('now'),
            datetime('now')
        FROM PackageDefinition
        WHERE PackageName='SOP8';
        """;


        command.ExecuteNonQuery();
    }
}
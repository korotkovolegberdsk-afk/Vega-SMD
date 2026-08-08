using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class ComponentDefinitionRepository
{
    public List<ComponentDefinition> GetAll()
    {
        var result = new List<ComponentDefinition>();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            c.Id,
            c.ManufacturerPartNumber,
            c.Description,
            c.PackageId,
            c.ComponentType,
            c.Manufacturer,
            c.Version,
            c.CreatedAt,
            c.UpdatedAt,
            p.PackageName,
            p.DisplayName
        FROM ComponentDefinition c
        INNER JOIN PackageDefinition p
            ON p.Id = c.PackageId;
        """;


        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }



    public List<ComponentDefinitionView> GetComponentViews()
    {
        var result = new List<ComponentDefinitionView>();

        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            c.Id,
            c.ManufacturerPartNumber,
            c.Manufacturer,
            c.ComponentType,
            c.Description,

            p.PackageName,

            cat.Name AS CategoryName,

            f.Name AS FamilyName

        FROM ComponentDefinition c

        INNER JOIN PackageDefinition p
            ON p.Id = c.PackageId

        LEFT JOIN PackageCategory cat
            ON cat.Id = p.CategoryId

        LEFT JOIN PackageFamily f
            ON f.Id = p.FamilyId;
        """;


        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(
                new ComponentDefinitionView
                {
                    Id = reader.GetInt32(0),

                    ManufacturerPartNumber =
                        reader.GetString(1),

                    Manufacturer =
                        reader.IsDBNull(2)
                            ? ""
                            : reader.GetString(2),

                    ComponentType =
                        reader.IsDBNull(3)
                            ? ""
                            : reader.GetString(3),

                    Description =
                        reader.IsDBNull(4)
                            ? ""
                            : reader.GetString(4),

                    PackageName =
                        reader.IsDBNull(5)
                            ? ""
                            : reader.GetString(5),

                    PackageCategory =
                        reader.IsDBNull(6)
                            ? ""
                            : reader.GetString(6),

                    PackageFamily =
                        reader.IsDBNull(7)
                            ? ""
                            : reader.GetString(7)
                });
        }


        return result;
    }



    public ComponentDefinition? GetById(int id)
    {
        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            c.Id,
            c.ManufacturerPartNumber,
            c.Description,
            c.PackageId,
            c.ComponentType,
            c.Manufacturer,
            c.Version,
            c.CreatedAt,
            c.UpdatedAt,
            p.PackageName,
            p.DisplayName
        FROM ComponentDefinition c
        INNER JOIN PackageDefinition p
            ON p.Id = c.PackageId
        WHERE c.Id = $id;
        """;


        command.Parameters.AddWithValue(
            "$id",
            id);


        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return Map(reader);
        }

        return null;
    }



    public void Add(ComponentDefinition component)
    {
        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO ComponentDefinition
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
        VALUES
        (
            $mpn,
            $description,
            $packageId,
            $type,
            $manufacturer,
            $version,
            $created,
            $updated
        );
        """;


        command.Parameters.AddWithValue(
            "$mpn",
            component.ManufacturerPartNumber);

        command.Parameters.AddWithValue(
            "$description",
            component.Description ?? "");

        command.Parameters.AddWithValue(
            "$packageId",
            component.PackageId);

        command.Parameters.AddWithValue(
            "$type",
            component.ComponentType ?? "");

        command.Parameters.AddWithValue(
            "$manufacturer",
            component.Manufacturer ?? "");

        command.Parameters.AddWithValue(
            "$version",
            component.Version);

        command.Parameters.AddWithValue(
            "$created",
            DateTime.Now);

        command.Parameters.AddWithValue(
            "$updated",
            DateTime.Now);


        command.ExecuteNonQuery();
    }



    public void Update(ComponentDefinition component)
    {
        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE ComponentDefinition
        SET
            ManufacturerPartNumber = $mpn,
            Description = $description,
            PackageId = $packageId,
            ComponentType = $type,
            Manufacturer = $manufacturer,
            Version = Version + 1,
            UpdatedAt = $updated
        WHERE Id = $id;
        """;


        command.Parameters.AddWithValue("$id", component.Id);
        command.Parameters.AddWithValue("$mpn", component.ManufacturerPartNumber);
        command.Parameters.AddWithValue("$description", component.Description ?? "");
        command.Parameters.AddWithValue("$packageId", component.PackageId);
        command.Parameters.AddWithValue("$type", component.ComponentType ?? "");
        command.Parameters.AddWithValue("$manufacturer", component.Manufacturer ?? "");
        command.Parameters.AddWithValue("$updated", DateTime.Now);


        command.ExecuteNonQuery();
    }

     

    public void Delete(int id)
    {
        using var connection = MasterLibraryConnection.Create();

        using var command = connection.CreateCommand();

        command.CommandText =
        """
        DELETE FROM ComponentDefinition
        WHERE Id = $id;
        """;


        command.Parameters.AddWithValue(
            "$id",
            id);


        command.ExecuteNonQuery();
    }



    private static ComponentDefinition Map(
        SqliteDataReader reader)
    {
        return new ComponentDefinition
        {
            Id = reader.GetInt32(0),

            ManufacturerPartNumber =
                reader.GetString(1),

            Description =
                reader.IsDBNull(2)
                    ? ""
                    : reader.GetString(2),

            PackageId =
                reader.GetInt32(3),

            ComponentType =
                reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4),

            Manufacturer =
                reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5),

            Version =
                reader.GetInt32(6),

            Package = new PackageDefinition
            {
                Id = reader.GetInt32(3),
                PackageName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                DisplayName = reader.IsDBNull(10) ? "" : reader.GetString(10)
            }
        };
    }
}
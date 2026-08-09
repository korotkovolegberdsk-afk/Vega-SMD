using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class PackageAliasRepository
{
    public List<PackageAlias> GetByPackageId(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, Alias, AliasType, Source, IsActive FROM MasterLibrary_PackageAliases WHERE PackageId = $packageId AND IsActive = 1 ORDER BY Alias;";
        command.Parameters.AddWithValue("$packageId", packageId);
        using var reader = command.ExecuteReader();
        var aliases = new List<PackageAlias>();
        while (reader.Read()) aliases.Add(Map(reader));
        return aliases;
    }

    public List<PackageDefinition> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT p.Id, p.PackageName, p.DisplayName, p.StandardName, p.PackageFamily, p.ComponentType,
                   p.IPCName, p.JEDECName, p.YamahaName, p.Description, p.IsActive
            FROM PackageDefinition p
            LEFT JOIN MasterLibrary_PackageAliases a ON a.PackageId = p.Id AND a.IsActive = 1
            WHERE p.IsActive = 1 AND (
                p.PackageName LIKE $query COLLATE NOCASE OR p.StandardName LIKE $query COLLATE NOCASE OR
                p.IPCName LIKE $query COLLATE NOCASE OR p.JEDECName LIKE $query COLLATE NOCASE OR
                p.YamahaName LIKE $query COLLATE NOCASE OR a.Alias LIKE $query COLLATE NOCASE)
            ORDER BY p.PackageName;
            """;
        command.Parameters.AddWithValue("$query", $"%{query.Trim()}%");
        using var reader = command.ExecuteReader();
        var packages = new List<PackageDefinition>();
        while (reader.Read()) packages.Add(new PackageDefinition
        {
            Id = reader.GetInt32(0), PackageName = reader.GetString(1), DisplayName = reader.GetString(2), StandardName = reader.GetString(3),
            PackageFamily = reader.GetString(4), ComponentType = reader.GetString(5), IPCName = reader.GetString(6), JEDECName = reader.GetString(7),
            YamahaName = reader.GetString(8), Description = reader.GetString(9), IsActive = reader.GetInt32(10) != 0
        });
        return packages;
    }

    public bool IsAliasUnique(int packageId, string alias, int excludeId = 0) => !GetByPackageId(packageId).Any(a => a.Id != excludeId && a.Alias.Equals(alias.Trim(), StringComparison.OrdinalIgnoreCase));
    public PackageAlias CreateAlias(PackageAlias alias) { alias.Alias = alias.Alias.Trim(); if (!IsAliasUnique(alias.PackageId, alias.Alias)) throw new InvalidOperationException("Duplicate alias."); Add(alias); return GetByPackageId(alias.PackageId).Single(a => a.Alias.Equals(alias.Alias, StringComparison.OrdinalIgnoreCase)); }
    public void DeleteAlias(int id) { using var connection=MasterLibraryConnection.Create(); using var command=connection.CreateCommand(); command.CommandText="DELETE FROM MasterLibrary_PackageAliases WHERE Id=$id;"; command.Parameters.AddWithValue("$id",id); command.ExecuteNonQuery(); }

    public void Add(PackageAlias alias)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO MasterLibrary_PackageAliases (PackageId, Alias, AliasType, Source, IsActive) VALUES ($packageId, $alias, $aliasType, $source, $isActive);";
        AddParameters(command, alias); command.ExecuteNonQuery();
    }

    public void Update(PackageAlias alias)
    {
        using var connection = MasterLibraryConnection.Create(); using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MasterLibrary_PackageAliases SET Alias=$alias, AliasType=$aliasType, Source=$source, IsActive=$isActive WHERE Id=$id;";
        alias.Alias = alias.Alias.Trim(); if (!IsAliasUnique(alias.PackageId, alias.Alias, alias.Id)) throw new InvalidOperationException("Duplicate alias."); AddParameters(command, alias); command.Parameters.AddWithValue("$id", alias.Id); command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, PackageAlias alias)
    { command.Parameters.AddWithValue("$packageId", alias.PackageId); command.Parameters.AddWithValue("$alias", alias.Alias); command.Parameters.AddWithValue("$aliasType", alias.AliasType); command.Parameters.AddWithValue("$source", alias.Source); command.Parameters.AddWithValue("$isActive", alias.IsActive); }
    private static PackageAlias Map(SqliteDataReader r) => new() { Id=r.GetInt32(0), PackageId=r.GetInt32(1), Alias=r.GetString(2), AliasType=r.GetString(3), Source=r.GetString(4), IsActive=r.GetInt32(5)!=0 };
}
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageDocumentRepository
{
    public List<PackageDocument> GetByPackageId(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, DocumentType, FileName, FilePath, Description FROM MasterLibrary_PackageDocuments WHERE PackageId = $packageId ORDER BY DocumentType, FileName;";
        command.Parameters.AddWithValue("$packageId", packageId);
        return Read(command);
    }

    public int Add(PackageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description) VALUES ($packageId, $documentType, $fileName, $filePath, $description); SELECT last_insert_rowid();";
        AddParameters(command, document);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(PackageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MasterLibrary_PackageDocuments SET DocumentType=$documentType, FileName=$fileName, FilePath=$filePath, Description=$description WHERE Id=$id;";
        AddParameters(command, document);
        command.Parameters.AddWithValue("$id", document.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM MasterLibrary_PackageDocuments WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static List<PackageDocument> Read(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var documents = new List<PackageDocument>();
        while (reader.Read()) documents.Add(new PackageDocument
        {
            Id = reader.GetInt32(0), PackageId = reader.GetInt32(1),
            DocumentType = Enum.Parse<PackageDocumentType>(reader.GetString(2), true),
            FileName = reader.GetString(3), FilePath = reader.GetString(4), Description = reader.GetString(5)
        });
        return documents;
    }

    private static void AddParameters(SqliteCommand command, PackageDocument document)
    {
        command.Parameters.AddWithValue("$packageId", document.PackageId); command.Parameters.AddWithValue("$documentType", document.DocumentType.ToString());
        command.Parameters.AddWithValue("$fileName", document.FileName); command.Parameters.AddWithValue("$filePath", document.FilePath); command.Parameters.AddWithValue("$description", document.Description);
    }
}
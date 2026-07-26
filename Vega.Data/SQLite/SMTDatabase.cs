using Microsoft.Data.Sqlite;

namespace Vega.Data.SQLite;

public class SMTDatabase
{
    private readonly string _connectionString;


    public SMTDatabase()
    {
        _connectionString = "Data Source=SMT.db";
    }


    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Packages
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,

            PackageName TEXT NOT NULL UNIQUE,
            DisplayName TEXT,

            Category TEXT,
            Family TEXT,

            Length REAL,
            Width REAL,
            Height REAL,

            Pitch REAL,
            LeadCount INTEGER,

            IPCName TEXT,
            JEDECName TEXT,

            YamahaName TEXT,
            MirtecName TEXT,

            StencilThickness REAL,
            AreaRatio REAL,
            AspectRatio REAL,

            ApertureType TEXT,

            TypicalDefects TEXT,
            AOIRecommendations TEXT,
            SPIRecommendations TEXT,

            Notes TEXT
        );
        """;


        command.ExecuteNonQuery();
    }
}